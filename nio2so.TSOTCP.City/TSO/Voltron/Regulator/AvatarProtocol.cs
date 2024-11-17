using nio2so.Formats.DB;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.Collections;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator("AvatarProtocol")]
    internal class AvatarProtocol : ITSOProtocolRegulator
    {
        public string RegulatorName => "AvatarProtocol";

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            IEnumerable<TSOVoltronPacket> SplitBlob(TSOVoltronPacket DBWrapper)
            {
                List<TSOVoltronPacket> packets = new();
                if (DBWrapper.BodyLength > 10000)//TSOSplitBufferPDU.STANDARD_CHUNK_SIZE)
                    packets.AddRange(TSOPDUFactory.CreateSplitBufferPacketsFromPDU(DBWrapper));
                else packets.Add(DBWrapper);
                return packets;
            }

            List<TSOVoltronPacket> returnPackets = new();
            Response = new(returnPackets, null, null);

            void EnqueuePacket(TSOVoltronPacket PacketToEnqueue) => returnPackets.AddRange(SplitBlob(PacketToEnqueue));

            switch ((TSO_PreAlpha_DBStructCLSIDs)PDU.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage:
                    { // TSO Net Message Standard type responses (most common)
                        var clsID = (TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID;
                        switch (clsID)
                        {
                            case TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Request:
                                {                                    
                                    var avatarID = PDU.Data1.Value;
                                    string AvatarName = avatarID == 1337 ? "JollySim" : "Bisquick";
                                    string AvatarDescription = "This is a sim description.";

                                    var charid = new TSOGetCharByIDResponse(PDU.AriesID, PDU.MasterID, avatarID,
                                        AvatarName, AvatarDescription);
                                    EnqueuePacket(charid);
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Request:
                                {
                                    var avatarID = PDU.Data1.Value;
                                    var charBlob = TSOFactoryBase.Get<TSOAvatarFactory>().GetCharBlobByID(avatarID);
                                    if (charBlob == null)
                                        throw new NullReferenceException($"CharBlob {avatarID} is null and unhandled.");

                                    TSOCityTelemetryServer.Global.OnCharBlob(NetworkTrafficDirections.OUTBOUND,avatarID, charBlob);
                                    var response = new TSOGetCharBlobByIDResponse(PDU.AriesID, PDU.MasterID, avatarID,charBlob);
                                    EnqueuePacket(response);
                                }
                                return true;
                            //The client is asking for bookmarks for the given avatar from the remote server
                            case TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Request:
                                {                                    
                                    var avatarID = PDU.Data1.Value;
                                    if (avatarID == 0) return false;

                                    EnqueuePacket(new TSOGetBookmarksResponse(PDU.AriesID,
                                                                              PDU.MasterID,
                                                                              avatarID,
                                                                              0,
                                                                              161));//,142,145)); // Add more to test Bookmarks
                                }
                                return true;
                            // The client is attempting to send us a newly created Avatar blob
                            // We will take the new data and add it to our pseudo-database for later usage
                            case TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request:
                                {
                                    //Data1 is the avatarID -- this is given by the Shard-Selector servlet
                                    var avatarID = PDU.Data1.Value;
                                    if (avatarID == 0)
                                        throw new InvalidDataException("You cannot provide zero as an AvatarID when sending a Client to CAS. " +
                                            "Give the Client an actual AvatarID. Ignoring this PDU.");

                                    PDU.ReadAdditionalMetadata(); // move packet position to end of metadata                                    
                                    var blob = PDU.ReadToEnd(); // read from metadata to EOF (or packet, in this case?)
                                    TSODBCharBlob charBlob = new(blob);

                                    //log this to disk
                                    TSOCityTelemetryServer.Global.OnCharBlob(NetworkTrafficDirections.INBOUND, avatarID, charBlob);
                                    EnqueuePacket(new TSOInsertCharBlobByIDResponse(PDU.AriesID, PDU.MasterID, avatarID));
                                }
                                return true;
                            // The client is attempting to set the data at a specific avatarID to be the payload of the packet
                            case TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request:
                                {
                                    var avatarID = PDU.Data1.Value;
                                    if (avatarID == 0)
                                        throw new InvalidDataException($"{TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request} AvatarID: {avatarID}. ERROR!!!");

                                    PDU.ReadAdditionalMetadata(); // move packet position to end of metadata                                    
                                    var blob = PDU.ReadToEnd(); // read from metadata to EOF (or packet, in this case?)
                                    TSODBChar charData = new(blob);

                                    //log this to disk
                                    TSOCityTelemetryServer.Global.OnCharData(NetworkTrafficDirections.INBOUND, avatarID, charData);
                                    EnqueuePacket(new TSOSetCharByIDResponse(PDU.AriesID, PDU.MasterID, avatarID));
                                }
                                return true;
                            // Logs a new line to the remote console on the server
                            case TSO_PreAlpha_DBActionCLSIDs.InsertGenericLog_Request:
                                string message = PDU.MessageString;
                                TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message, 
                                    "cDBServiceClientD", "[InsertGenericLog_Request] " + message));
                                return true;
                        }
                    }
                    break;
            }

            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> returnPackets = new List<TSOVoltronPacket>();
            Response = new(returnPackets,null,null);

            bool success = false;
            void defaultSend(TSOVoltronPacket outgoing)
            {
                returnPackets.Add(outgoing);
                success = true;
            }
            void logme(TSOVoltronPacket statusUpdater)
            {
                TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                    RegulatorName, $"[STATUS_UPDATED] {statusUpdater}"));
                success = true;
            }

            switch (PDU.KnownPacketType)
            {
                // Called when pressing Find Person in the AvatarPage or when loading into the game
                // during PreLoadDBContent
                case TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU:
                    {
                        var formattedPacket = (TSOFindPlayerPDU)PDU;
                        if (uint.TryParse(formattedPacket.AriesID.Substring(
                            formattedPacket.AriesID.IndexOf("A ") + 2), out uint AvatarID))
                        {
                            defaultSend(new TSOFindPlayerResponsePDU());
                            TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                                RegulatorName, $"FIND PLAYER: {AvatarID}"));
                            break;
                        }
                        TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                                RegulatorName, $"FIND PLAYER: ERROR! {formattedPacket.AriesID} ???"));
                    }
                    break;
                // Sent when a player creates a new Avatar from CAS
                case TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_CREATEAVATARNOTIFICATION_PDU:
                    {
                        TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                            RegulatorName, $"({TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_CREATEAVATARNOTIFICATION_PDU}) Avatar was created."));
                    }
                    return true;
                //**STATUS UPDATERS**
                //These right now don't qualify a response so they're just logged.
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU:
                    //defaultSend(new TSOSetAcceptAlertsResponsePDU(true));                                        
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU:
                    //defaultSend(new TSOSetAcceptFlashesResponsePDU(true));                    
                case TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU:
                    //defaultSend(new TSOSetIgnoreListResponsePDU(true));                    
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU:
                    //defaultSend(new TSOSetInvincibleResponsePDU(true));                    
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU:
                    //defaultSend(new TSOSetInvisibleResponsePDU(true));

                    //** below each case is the response value commented out
                    // if a time comes where these are useful then you can uncomment them

                    logme(PDU); // just log it here for now.
                    break;
            }

            return success; 
        }
    }
}

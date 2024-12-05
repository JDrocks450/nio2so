using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.Telemetry;
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
    [TSORegulator("LotProtocol")]
    internal class LotProtocol : ITSOProtocolRegulator
    {
        private uint _houseCreateX = 83, _houseCreateY = 157; // ocean island

        public string RegulatorName => "LotProtocol";

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            IEnumerable<TSOVoltronPacket> SplitBlob(TSOVoltronPacket DBWrapper)
            {
                List<TSOVoltronPacket> packets = new();
                if (DBWrapper.BodyLength > TSOSplitBufferPDU.STANDARD_CHUNK_SIZE)
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
                        switch ((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID)
                        {
                            //Avatar wants to buy a cool new lot, you know?
                            case TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Request:
                                {
                                    TSOBuyLotByAvatarIDRequest lotPurchasePDU = (TSOBuyLotByAvatarIDRequest)PDU;
                                    uint? NewID = TSOFactoryBase.Get<TSOHouseFactory>()?.Create();
                                    if (!NewID.HasValue) throw new NullReferenceException("Factory could not be mapped or there was an error creating a house.");
                                    TSOVoltronPacket buyPDU = new TSOBuyLotByAvatarIDResponse(NewID.Value,
                                                                                              TestingConstraints.BuyLotEndingFunds,
                                                                                              lotPurchasePDU.Lot_X,
                                                                                              lotPurchasePDU.Lot_Y);
                                    TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message, RegulatorName,
                                        "Congrats on the new property. Your GetLotList_Response has been updated to include your new place."));
                                    _houseCreateX = lotPurchasePDU.Lot_X;
                                    _houseCreateY = lotPurchasePDU.Lot_Y;

                                    returnPackets.Add(buyPDU);
                                }
                                return true;
                            // Gets information about the roommates on a given lot
                            case TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request:
                                {
                                    var roommatePDU = (TSOGetRoommateInfoByLotIDRequest)PDU;
                                    uint HouseID = roommatePDU.HouseID;
                                    if (HouseID == 0) 
                                        return false; // Seems to be mistaken to send in this scenario
                                    returnPackets.Add(new TSOGetRoommateInfoByLotIDResponse(HouseID,
                                        TestingConstraints.MyAvatarID, TestingConstraints.MyFriendAvatarID));                                        
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request:
                                {
                                    returnPackets.Add(new TSOGetLotListResponse(TestingConstraints.BuyLotID, _houseCreateX, _houseCreateY));
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request:
                            // Gets a TSODataDefinition Lot struct with the LotID provided
                                {
                                    returnPackets.Add(TSODebugWrapperPDU.FromFile(@"E:\packets\const\GetLotByID_Response.dat",
                                        TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Response));
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request:
                                {
                                    uint HouseID = ((TSOGetHouseLeaderByIDRequest)PDU).HouseID;
                                    returnPackets.Add(new TSOGetHouseLeaderByIDResponse(HouseID, TestingConstraints.MyAvatarID));
                                }
                                return true;
                            // Requests a HouseBlob for the given HouseID
                            case TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request:
                                {
                                    //log debug mode
                                    TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Warnings,
                                        RegulatorName, nameof(TestingConstraints.JustGetMeToLotView) + " is " +
                                        TestingConstraints.JustGetMeToLotView));

                                    var housePacket = (TSOGetHouseBlobByIDRequest)PDU;
                                    uint HouseID = housePacket.HouseID;
                                    //__TESTING MODE__
                                    uint loadHouseID = TestingConstraints.JustGetMeToLotView ? 1 : HouseID;
                                    //**
                                    TSODBHouseBlob? houseBlob = TSOFactoryBase.Get<TSOHouseFactory>()?.GetHouseBlobByID(loadHouseID);
                                    if (houseBlob == null)
                                        throw new NullReferenceException($"HouseBlob {HouseID} is null and unhandled.");
                                    //**TESTING MODE**
                                    TSOVoltronPacket response = TestingConstraints.JustGetMeToLotView ?
                                        new TSOGetHouseBlobByIDResponseDEBUG(HouseID, houseBlob) :
                                        new TSOGetHouseBlobByIDResponseTEST(HouseID, houseBlob);
                                    //***
                                    EnqueuePacket(response);                                        
                                }
                                return true;
                            // The Client is requesting to set the house data for this HouseID in the Database
                            case TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request:
                                { // client is giving us house blob data
                                    TSOSetHouseBlobByIDRequest housePDU = (TSOSetHouseBlobByIDRequest)PDU;

                                    uint HouseID = housePDU.HouseID;
                                    var blob = housePDU.StreamBytes;
                                    TSODBHouseBlob houseBlob = new(blob);

                                    HouseID = 6057;

                                    //log this to disk
                                    TSOCityTelemetryServer.Global.OnHouseBlob(NetworkTrafficDirections.INBOUND, HouseID, houseBlob);
                                }
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

            switch (PDU.KnownPacketType)
            {                
                case TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU:
                    {
                        defaultSend(new TSOLoadHouseResponsePDU(TestingConstraints.MyHouseID));
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU:
                    {
                        /* From niotso:                  hello, fatbag - bisquick :]
                         * TODO: It is known that sending HouseSimConstraintsResponsePDU (before the
	                    ** GetCharBlobByID response and other packets) is necessary for the game to post
	                    ** kMSGID_LoadHouse and progress HouseLoadRegulator from kStartedState to kLoadingState.
	                    ** However, the game appears to never send HouseSimConstraintsPDU to the server at
	                    ** any point.
	                    **
	                    ** The question is:
	                    ** Is there a packet that we can send to the game to have it send us
	                    ** HouseSimConstraintsPDU?
	                    ** Actually, (in New & Improved at least), manually sending a kMSGID_LoadHouse packet
	                    ** to the client (in an RSGZWrapperPDU) will cause the client to send
	                    ** HouseSimConstraintsPDU to the server.
	                    ** It is not known at this point if that is the "correct" thing to do.
	                    **
	                    ** So, for now, we will send the response packet to the game, without it having explicitly
	                    ** sent us a request packet--just like (in response to HostOnlinePDU) the game sends us a
	                    ** LoadHouseResponsePDU without us ever having sent it a LoadHousePDU. ;)
	                    */

                        success = true;
                        var houseID = ((TSOLoadHouseResponsePDU)PDU).HouseID;

                        //The client will always send a LoadHouseResponsePDU with a blank houseID, so this can be ignored
                        //when that happens
                        if (houseID == 0 && !TestingConstraints.LOTTestingMode)
                            break;// break;

                        defaultSend(new TSOHouseSimConstraintsResponsePDU(TSOVoltronConst.MyHouseID)); // dictate what lot to load here.                        
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_PDU:
                    {
                        defaultSend(new TSOListRoomsResponsePDU());
                    }
                    break;
                // Sent when the client would like to enter into a Room Server
                case TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU:
                    {
                        var roomPDU = (TSOLotEntryRequestPDU)PDU;
                        byte[] body = File.ReadAllBytes(@"E:\packets\const\UpdateRoomPDU.dat");
                        defaultSend(new TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU, body));
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU:
                    {
                        var msg = (TSOChatMessagePDU)PDU;
                        defaultSend(new TSOChatMessageFailedPDU(msg.Message));
                    }
                    break;
            }
            return success;
        }
    }
}

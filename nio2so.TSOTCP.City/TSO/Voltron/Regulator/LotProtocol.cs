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
        public string RegulatorName => "LotProtocol";

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> returnPackets = new();
            Response = new(returnPackets, null, null);

            switch ((TSO_PreAlpha_DBStructCLSIDs)PDU.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage:
                    { // TSO Net Message Standard type responses (most common)
                        switch ((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID)
                        {
                            // Gets information about the roommates on a given lot
                            case TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request:
                                {
                                    var roommatePDU = (TSOGetRoommateInfoByLotIDRequest)PDU;
                                    uint HouseID = roommatePDU.HouseID;
                                    if (HouseID == 0) 
                                        return false; // Seems to be mistaken to send in this scenario
                                    returnPackets.Add(new TSOGetRoommateInfoByLotIDResponse(HouseID, TestingConstraints.MyFriendAvatarID));                                        
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request:
                                {
                                    returnPackets.Add(new TSOGetLotListResponse(TestingConstraints.MyHouseID));
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request:
                                // Gets a TSODataDefinition Lot struct with the LotID provided
                                {

                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request:
                                {
                                    uint HouseID = 0;// PDU.Data1.Value; // DATA1 is HouseID
                                    returnPackets.Add(new TSOGetHouseLeaderByIDResponse(PDU.AriesID,PDU.MasterID,HouseID, TestingConstraints.MyFriendAvatarID));
                                }
                                return true;
                            // Requests a HouseBlob for the given HouseID
                            case TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request:
                                {
                                    var housePacket = (TSOGetHouseBlobByIDRequest)PDU;
                                    uint HouseID = housePacket.HouseID; 
                                    TSODBHouseBlob? houseBlob = TSOFactoryBase.Get<TSOHouseFactory>()?.GetHouseBlobByID(0);
                                    if (houseBlob == null)
                                        throw new NullReferenceException($"HouseBlob {HouseID} is null and unhandled.");

                                    returnPackets.Add(new TSOGetHouseBlobByIDResponse(HouseID, houseBlob));                                        
                                }
                                return true;
                            // The Client is requesting to set the house data for this HouseID in the Database
                            case TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request:
                                { // client is giving us house blob data
                                    uint HouseID = 0;// PDU.Data1.Value; // DATA1 is HouseID
                                    //PDU.ReadAdditionalMetadata(); // move packet position to end of metadata
                                    PDU.SetPosition(0x29); // end of metadata
                                    var blob = PDU.ReadToEnd();
                                    TSODBHouseBlob houseBlob = new(blob);
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
                        defaultSend(new TSOLoadHouseResponsePDU());
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
            }
            return success;
        }
    }
}

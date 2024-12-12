using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
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
    [TSORegulator]
    internal class LotProtocol : TSOProtocol
    {
        private uint _houseCreateX = 83, _houseCreateY = 157; // ocean island

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Request)]
        public void BuyLotByAvatarID_Request(TSODBRequestWrapper PDU)
        {
            TSOBuyLotByAvatarIDRequest lotPurchasePDU = (TSOBuyLotByAvatarIDRequest)PDU;
            uint? NewID = TestingConstraints.BuyLotID;//TSOFactoryBase.Get<TSOHouseFactory>()?.Create();
            if (!NewID.HasValue) throw new NullReferenceException("Factory could not be mapped or there was an error creating a house.");
            TSODBRequestWrapper buyPDU = new TSOBuyLotByAvatarIDResponse(NewID.Value,
                                                                      TestingConstraints.BuyLotEndingFunds,
                                                                      lotPurchasePDU.Lot_X,
                                                                      lotPurchasePDU.Lot_Y);
            TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message, RegulatorName,
                "Congrats on the new property. Your GetLotList_Response has been updated to include your new place."));
            _houseCreateX = lotPurchasePDU.Lot_X;
            _houseCreateY = lotPurchasePDU.Lot_Y;

            RespondTo(PDU, buyPDU);
        }

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request)]
        public void GetRoommateInfoByLotID_Request(TSODBRequestWrapper PDU)
        {
            var roommatePDU = (TSOGetRoommateInfoByLotIDRequest)PDU;
            uint HouseID = roommatePDU.HouseID;
            if (HouseID == 0)
                return; // Seems to be mistaken to send in this scenario

            RespondTo(roommatePDU, new TSOGetRoommateInfoByLotIDResponse(HouseID,
                TestingConstraints.MyAvatarID, TestingConstraints.MyFriendAvatarID));
        }

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request)]
        public void GetLotList_Request(TSODBRequestWrapper PDU)
        {
            RespondTo(PDU, new TSOGetLotListResponse(TestingConstraints.BuyLotID, _houseCreateX, _houseCreateY));
        }

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request)]
        public void GetLotByID_Request(TSODBRequestWrapper PDU)        
        { // Gets a TSODataDefinition Lot struct with the LotID provided
            RespondTo(PDU, TSODebugWrapperPDU.FromFile(@"E:\packets\const\GetLotByID_Response.dat",
                TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Response));
        }

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
        public void GetHouseLeaderByLotID_Request(TSODBRequestWrapper PDU)
        {
            uint HouseID = ((TSOGetHouseLeaderByIDRequest)PDU).HouseID;
            RespondTo(PDU, new TSOGetHouseLeaderByIDResponse(HouseID, TestingConstraints.MyAvatarID));
        }

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request)]
        public void GetHouseBlobByID_Request(TSODBRequestWrapper PDU)
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
            RespondWith(response);           
            RespondWith(new TSOUpdatePlayerPDU(TSOVoltronConst.MyAvatarID, TSOVoltronConst.MyAvatarName));
            RespondWith(new TSOHouseSimConstraintsResponsePDU(HouseID)); // dictate what lot to load here.
            byte[] fileData = File.ReadAllBytes(@"E:\packets\const\JoinAndCreateRoomPDU.dat");
            RespondWith(new TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes.CREATE_AND_JOIN_ROOM_FAILED_PDU,fileData));
            //RespondWith(new TSOOccupantArrivedPDU(TestingConstraints.MyFriendAvatarID, TestingConstraints.MyFriendAvatarName));
        }

        // The Client is requesting to set the house data for this HouseID in the Database
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request)]
        public void SetHouseBlobByID_Request(TSODBRequestWrapper PDU)
        { // client is giving us house blob data
            TSOSetHouseBlobByIDRequest housePDU = (TSOSetHouseBlobByIDRequest)PDU;

            uint HouseID = housePDU.HouseID;
            var blob = housePDU.StreamBytes;
            TSODBHouseBlob houseBlob = new(blob);

            HouseID = 6057;

            //log this to disk
            TSOCityTelemetryServer.Global.OnHouseBlob(NetworkTrafficDirections.INBOUND, HouseID, houseBlob);
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
        public void LOAD_HOUSE_PDU(TSOVoltronPacket PDU)
        {
            RespondWith(new TSOLoadHouseResponsePDU(TestingConstraints.MyHouseID));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
        public void LOAD_HOUSE_RESPONSE_PDU(TSOVoltronPacket PDU)
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

            var houseID = ((TSOLoadHouseResponsePDU)PDU).HouseID;

            //The client will always send a LoadHouseResponsePDU with a blank houseID, so this can be ignored
            //when that happens
            if (houseID == 0 && !TestingConstraints.LOTTestingMode)
                return; // break;

            RespondWith(new TSOHouseSimConstraintsResponsePDU(TSOVoltronConst.MyHouseID)); // dictate what lot to load here.                        
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_PDU)]
        public void LIST_ROOMS_PDU(TSOVoltronPacket PDU)
        {
            RespondWith(new TSOListRoomsResponsePDU());
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
        public void LOT_ENTRY_REQUEST_PDU(TSOVoltronPacket PDU)
        {
            var roomPDU = (TSOLotEntryRequestPDU)PDU;
            byte[] body = File.ReadAllBytes(@"E:\packets\const\UpdateRoomPDU.dat");
            RespondWith(new TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU, body));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU)]
        public void BROADCAST_DATABLOB_PDU(TSOVoltronPacket PDU)
        {
            var broadcastPDU = (TSOBroadcastDatablobPDU)PDU;
            RespondWith(broadcastPDU);
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
        public void CHAT_MSG_PDU(TSOVoltronPacket PDU)
        {
            var msg = (TSOChatMessagePDU)PDU;
            RespondWith(new TSOChatMessageFailedPDU(msg.Message));
        }
    }
}

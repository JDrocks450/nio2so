﻿using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.Formats.FAR3;
using nio2so.Formats.Streams;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.City.TSO.Voltron.Serialization;
using nio2so.TSOTCP.HSBServer;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{   
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator]
    public class LotProtocol : TSOProtocol
    {
        bool _iminalot = false;
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
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message, RegulatorName,
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
                TestingConstraints.MyAvatarID,
                TestingConstraints.MyFriendAvatarID));
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
            var housePacket = (TSOGetHouseBlobByIDRequest)PDU;
            uint HouseID = housePacket.HouseID;
            //__TESTING MODE__
            uint loadHouseID = HouseID;
            //**
            //Read decompressed House Blob from disk
            TSODBHouseBlob? houseBlob = TSOFactoryBase.Get<TSOHouseFactory>()?.GetHouseBlobByID(loadHouseID);
            if (houseBlob == null)
                throw new NullReferenceException($"HouseBlob {HouseID} is null and unhandled.");

            var response = new TSOGetHouseBlobByIDResponse(HouseID, houseBlob, true);            
            RespondTo(PDU,response);

            if (Server == HSBSession.RoomServer)
                return;
            if (TestingConstraints.LOTTestingMode) return;

            if (!_iminalot)
            {
                _iminalot = true;
                var kClientConnectedMsg = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kClientHasConnected)
                );
                RespondTo(PDU, kClientConnectedMsg);                
            }
            else
            {
                //RespondWith(new TSOOccupantArrivedPDU(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName));
                return;
                var kClientConnectedMsg = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID,
                    new byte[4] { 0x00, 0x00, 0x05, 0x39 })
                );
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, kClientConnectedMsg);
                
            }

            //OLD CODE: ==========

            //RespondWith(new TSOHouseSimConstraintsResponsePDU(HouseID)); // dictate what lot to load here.
            //RespondWith(new TSOUpdatePlayerPDU(TSOVoltronConst.MyAvatarID, TSOVoltronConst.MyAvatarName));
        }

        // The Client is requesting to set the house data for this HouseID in the Database
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request)]
        public void SetHouseBlobByID_Request(TSODBRequestWrapper PDU)
        { // client is giving us house blob data
            TSOSetHouseBlobByIDRequest housePDU = (TSOSetHouseBlobByIDRequest)PDU;

            uint HouseID = housePDU.HouseID;
            if (!housePDU.TryUnpack(out Struct.SetHouseBlobByIDRequestStreamStructure? Structure)) // decompress the Serializable stream
                throw new InvalidDataException("Unable to read incoming HouseBlob data!");
            var blob = Structure.ChunkPackage.GetChunk(TSO_PreAlpha_HouseStreamChunkHeaders.hous);

            // write decompressed to the hard drive
            TSOServerTelemetryServer.Global.OnHouseBlob(NetworkTrafficDirections.INBOUND, HouseID, new(blob.Content));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
        public void LOAD_HOUSE_PDU(TSOVoltronPacket PDU)
        {            
            RespondWith(new TSOLoadHouseResponsePDU(TestingConstraints.MyHouseID));
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
            //byte[] body = File.ReadAllBytes(@"E:\packets\const\UpdateRoomPDU.dat");
            string LotName = "BloatyLand";
            string RoomName = LotName;
            TSOUpdateRoomPDU updateRoomPDU = new TSOUpdateRoomPDU(1, RoomName, roomPDU.HouseID,
                new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName),
                new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName));
            RespondWith(updateRoomPDU);

            //**INVOKE THE HSB
            if (HSBSession.HSB_Activated)
            {
                HSBSession.RoomServer.IsRunning = true;
            }
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
        public void CHAT_MSG_PDU(TSOVoltronPacket PDU)
        {
            var msg = (TSOChatMessagePDU)PDU;
            RespondWith(PDU);
            //RespondWith(new TSOChatMessageFailedPDU(msg.Message));
        }
    }
}
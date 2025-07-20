using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Util;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator]
    public class LotProtocol : TSOProtocol
    {
        record LotTest(uint ID, TSODBLotPosition Position, string PhoneNumber, string Name, string Description)
        {
            public string Name { get; set; } = Name;
            public string Description { get; set; } = Description;
        }             

        private static List<LotTest> _lots = new() {
            new(TestingConstraints.BuyLotID,new(93, 135), TestingConstraints.MyHousePhoneNumber, TestingConstraints.MyHouseName, "The first house profile working in nio2so. Check out that cool thumbnail.") // ocean island
        };

        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOBuyLotByAvatarIDRequest"/>
        /// <para/>Can either be <see cref="TSOBuyLotByAvatarIDResponse"/> or <see cref="TSOBuyLotByAvatarIDFailedResponse"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Request)]
        public void BuyLotByAvatarID_Request(TSODBRequestWrapper PDU)
        {
            TSOBuyLotByAvatarIDRequest lotPurchasePDU = (TSOBuyLotByAvatarIDRequest)PDU;
            
            //create failed packet
            void Failure() =>            
                RespondTo(PDU, new TSOBuyLotByAvatarIDFailedResponse());                                   
            
            //create success packet
            void Success()
            {
                long? NewID = TestingConstraints.BuyLotID + (_lots.Count);//TSOFactoryBase.Get<TSOHouseFactory>()?.Create();
                if (!NewID.HasValue) throw new NullReferenceException("Factory could not be mapped or there was an error creating a house.");
                TSODBRequestWrapper buyPDU = new TSOBuyLotByAvatarIDResponse((uint)NewID.Value,
                                                                          TestingConstraints.BuyLotEndingFunds,
                                                                          lotPurchasePDU.LotPosition);
                TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message, RegulatorName,
                    $"Lot Purchased: Owner: {lotPurchasePDU.AvatarID} HouseID: {NewID} Location: {lotPurchasePDU.LotPosition} String: {lotPurchasePDU.LotPhoneNumber}"));
                _lots.Add(new((uint)NewID, lotPurchasePDU.LotPosition, lotPurchasePDU.LotPhoneNumber, $"{PDU.SenderSessionID.MasterID}'s Place", "Please enter a description for your new property."));
                RespondTo(PDU, buyPDU);
            }

            Success();
        }

        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetRoommateInfoByLotIDRequest"/>
        /// <para/> For testing purposes, is always hardcoded to respond <see cref="TestingConstraints.MyAvatarID"/> is the owner, and <see cref="TestingConstraints.MyFriendAvatarID"/> is a roommate
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
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
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request"/>
        /// <para/>Responds with a new <see cref="TSOGetLotListResponse"/> with <see cref="TestingConstraints.BuyLotID"/> at the last clicked position on the map
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request)]
        public void GetLotList_Request(TSODBRequestWrapper PDU)
        {
            //send one packet for every lot in the world view
            foreach(var lot in _lots)
                RespondTo(PDU, new TSOGetLotListResponse(lot.ID, lot.Position));

            // ** You can send this PDU as many times as needed for each house to add to the map **
            //RespondTo(PDU, new TSOGetLotListResponse(TestingConstraints.BuyLotID+1, _houseCreateX+1, _houseCreateY+1));
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetLotByID_Request"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request)]
        public void GetLotByID_Request(TSODBRequestWrapper PDU)
        { // Gets information on the house with the LotID provided
            TSOGetLotByID_Request req = (TSOGetLotByID_Request)PDU;

            //get data
            LotTest? item = _lots.FirstOrDefault(lot => lot.ID == req.Lot_DatabaseID);
            if (item == null) throw new InvalidDataException($"LotID {req.Lot_DatabaseID} doesn't exist.");

            //send requested data
            RespondWith(new TSOGetLotByID_Response(req.Lot_DatabaseID, item.Name, "bisquick", item.Description, item.Position));
            
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Warnings, RegulatorName,
                $"GetLotByID_Request: ID: {req.Lot_DatabaseID}"));
        }
        /// <summary>
        /// Handles when a Client requests the PNG thumbnail image for a lot.
        /// <para/>Responds with a <see cref="TSOGetHouseThumbByIDResponse"/> containing the PNG image for the requested lot's thumbnail
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetHouseThumbByID_Request)]
        public void GetHouseThumbByID_Request(TSODBRequestWrapper PDU)
        {
            TSOGetHouseThumbByIDRequest req = (TSOGetHouseThumbByIDRequest)PDU;
            RespondWith(new TSOGetHouseThumbByIDResponse(req.HouseID, File.ReadAllBytes(@"E:\packets\house\special.png")));
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetHouseLeaderByIDRequest"/>
        /// <para/>Responds with a new <see cref="TSOGetHouseLeaderByIDResponse"/> with <see cref="TestingConstraints.MyAvatarID"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
        public void GetHouseLeaderByLotID_Request(TSODBRequestWrapper PDU)
        {
            uint HouseID = ((TSOGetHouseLeaderByIDRequest)PDU).HouseID;
            RespondTo(PDU, new TSOGetHouseLeaderByIDResponse(HouseID, TestingConstraints.MyAvatarID));
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetHouseBlobByIDRequest"/>
        /// <para/>Responds with a new <see cref="TSOGetHouseBlobByIDResponse"/> with the <see cref="TSODBHouseBlob"/> requested by its <c>HouseID</c>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
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
            RespondTo(PDU, response);            
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOSetHouseBlobByIDRequest"/>
        /// <para/>Responds with nothing. Saves the <see cref="TSODBHouseBlob"/> to file by its <c>HouseID</c>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        // The Client is requesting to set the house data for this HouseID in the Database
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request)]
        public void SetHouseBlobByID_Request(TSODBRequestWrapper PDU)
        { // client is giving us house blob data
            TSOSetHouseBlobByIDRequest housePDU = (TSOSetHouseBlobByIDRequest)PDU;

            uint HouseID = housePDU.HouseID;
            if (!housePDU.TryUnpack(out SetHouseBlobByIDRequestStreamStructure? Structure)) // decompress the Serializable stream
                throw new InvalidDataException("Unable to read incoming HouseBlob data!");
            File.WriteAllBytes("E:\\refpack.dat", housePDU.HouseFileStream.StreamContents);
            var blob = Structure.ChunkPackage.GetChunk(TSO_PreAlpha_HouseStreamChunkHeaders.hous);
            if (blob == null) // get hous chunk in the package
                throw new InvalidDataException("Unable to read incoming HouseBlob data!");
            // write decompressed to the hard drive
            TSOServerTelemetryServer.Global.OnHouseBlob(NetworkTrafficDirections.INBOUND, HouseID, new(blob.Content));
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOSetLotByIDRequest"/>
        /// <para/>Responds with nothing. Saves the <see cref="TSODBHouseBlob"/> to file by its <c>HouseID</c>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        // The Client is requesting to set the house data for this HouseID in the Database
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetLotByID_Request)]
        public void SetLotByID_Request(TSODBRequestWrapper PDU)
        { // client is giving us lot profile data
            TSOSetLotByIDRequest req = (TSOSetLotByIDRequest)PDU;

            //get data
            LotTest? item = _lots.FirstOrDefault(lot => lot.ID == req.LotID);
            if (item == null) throw new InvalidDataException($"LotID {req.LotID} doesn't exist.");

            //set data
            item.Name = req.LotName;
            item.Description = req.LotDescription;            

            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Warnings, RegulatorName,
                $"SetLotByID_Request: ID: {req.LotID}"));
        }

        /// <summary>
        /// Never seen sent to the server.
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
        public void LOAD_HOUSE_PDU(TSOVoltronPacket PDU)
        {
            throw new NotImplementedException();
            //RespondWith(new TSOLoadHouseResponsePDU(TestingConstraints.MyHouseID));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU)]
        public void DESTROY_ROOM_PDU(TSOVoltronPacket PDU)
        {
            var destroyRoomPDU = (TSODestroyRoomPDU)PDU;
            var myRoom = _lots[0];

            //Get the avatar who is trying to leave and destroy the room behind them
            uint RoomID = ((ITSONumeralStringStruct)destroyRoomPDU.RoomInfo)?.NumericID ?? 0;
            if (RoomID == 0)
                throw new InvalidDataException($"{nameof(DESTROY_ROOM_PDU)}(): {nameof(RoomID)} is {RoomID}! Ignoring...");

            RespondWith(new TSOUpdateRoomPDU(1, TSORoomInfoStruct.NoRoom));
            RespondWith(new TSODestroyRoomResponsePDU(TSOStatusReasonStruct.Success, new(myRoom.PhoneNumber,myRoom.Name)));
            
            return;

            //**tried these
            RespondWith(new TSODetachFromRoomPDU(new TSORoomIDStruct(myRoom.PhoneNumber,myRoom.Name)));
            
            var kClientConnectedMsg = new TSOBroadcastDatablobPacket(
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_UnloadHouse,
                    TSOVoltronSerializer.Serialize(new TSOAriesIDStruct(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName)))
                )
            {
                SenderSessionID = new TSOAriesIDStruct(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName)
            };
            kClientConnectedMsg.MakeBodyFromProperties();
            RespondWith(kClientConnectedMsg);
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_PDU)]
        public void LIST_ROOMS_PDU(TSOVoltronPacket PDU)
        {
            TSORoomInfoStruct[] rooms = new TSORoomInfoStruct[_lots.Count];
            int i = 0;
            foreach(var lot in _lots)
            {
                rooms[i++] = new TSORoomInfoStruct(
                    new TSORoomIDStruct(lot.PhoneNumber, lot.Name),
                    new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName), // change this later
                    (uint)i+1
                    );
            }
            RespondWith(new TSOListRoomsResponsePDU(rooms));
            return;
            //test message pdu ... doesn't work
            RespondWith(new TSOAnnouncementMsgPDU(new TSOPlayerInfoStruct(new(161, "FriendlyBuddy")), "Testing"));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
        public void LOT_ENTRY_REQUEST_PDU(TSOVoltronPacket PDU)
        {
            var roomPDU = (TSOLotEntryRequestPDU)PDU;

            // TELL THE CLIENT TO START THE HOST PROTOCOL
            RespondWith(new TSOHouseSimConstraintsResponsePDU(roomPDU.HouseID));            

            //get the lot
            LotTest? thisLot = _lots.FirstOrDefault(x => x.ID == roomPDU.HouseID);

            //get the phone number of the lot
            string phoneNumber = thisLot?.PhoneNumber ?? TestingConstraints.MyHousePhoneNumber;
            
            //Update which room they're in currently
            string LotName = thisLot?.Name ?? "BloatyLand";
            string RoomName = LotName;

            //create a list of admins
            var admins = new TSOAriesIDStruct[] { new TSOAriesIDStruct(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName) };
            uint occupants = (uint)admins.Length;

            //create the roominfo struct
            TSORoomInfoStruct roomInfo = new TSORoomInfoStruct(roomLocationInfo: new (phoneNumber, RoomName),
                ownerVector: new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName), // m_ownerID
                currentOccupants: occupants, maxOccupants: 10, isLocked: false,
                roomHostInformation: admins
            );

            //tell the client to join this new room
            TSOUpdateRoomPDU updateRoomPDU = new TSOUpdateRoomPDU(1, roomInfo);
            RespondWith(updateRoomPDU);            

            //**INVOKE THE HSB
            if (HSBSession.HSB_Activated)
            {

                var kClientConnectedMsg = new TSOBroadcastDatablobPacket(
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID,
                    TSOVoltronSerializer.Serialize(new TSOAriesIDStruct(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName)))
                )
                {
                    SenderSessionID = new TSOAriesIDStruct(TestingConstraints.HSBHostID, TestingConstraints.HSBHostName)
                };
                kClientConnectedMsg.MakeBodyFromProperties();
                HSBSession.RoomServer?.SendPacket(Server,
                        new TSOOccupantArrivedPDU(new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName)));
                HSBSession.RoomServer?.SendPacket(Server, kClientConnectedMsg);
            }            
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
        public void CHAT_MSG_PDU(TSOVoltronPacket PDU)
        {
            var msg = (TSOChatMessagePDU)PDU;
            RespondWith(msg);
            //RespondWith(new TSOChatMessageFailedPDU(msg.Message));
        }
    }
}

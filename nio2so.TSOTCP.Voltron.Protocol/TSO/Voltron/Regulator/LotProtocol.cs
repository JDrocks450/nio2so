using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using nio2so.Formats.DB;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Services;
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
        private static HashSet<uint> _onlineLotIDs = new HashSet<uint>();

        public LotProfile GetLotProfile(HouseIDToken HouseID)
        {
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");
            LotProfile? item = client.GetLotProfileByHouseID(HouseID).Result;
            if (item == null) throw new InvalidDataException($"LotID {HouseID} doesn't exist.");
            return item;
        }
        public TSOAriesIDStruct GetAvatarIDStruct(AvatarIDToken AvatarID)
        {
            var client = GetService<nio2soVoltronDataServiceClient>();
            return new(AvatarID, client.GetAvatarNameByAvatarID(AvatarID).Result);
        }

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

            //attempt to purchase this lot
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");

            LotProfile? newLotProfile = client.AttemptToPurchaseLotByAvatarID(lotPurchasePDU.AvatarID,
                lotPurchasePDU.LotPhoneNumber,lotPurchasePDU.LotPosition.X,lotPurchasePDU.LotPosition.Y).Result;

            //create failed packet
            void Failure() =>            
                RespondTo(PDU, new TSOBuyLotByAvatarIDFailedResponse());                                   
            
            //create success packet
            void Success()
            {
                //download my character data to get funds after the transaction
                TSODBChar myCharacterProfile = client.GetCharacterFileByAvatarID(lotPurchasePDU.AvatarID).Result;

                TSODBRequestWrapper buyPDU = new TSOBuyLotByAvatarIDResponse(newLotProfile.HouseID,myCharacterProfile.Funds,newLotProfile.Position);
                TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message, RegulatorName,
                    $"Lot Purchased: Owner: {lotPurchasePDU.AvatarID} HouseID: {newLotProfile.HouseID} Location: {newLotProfile.Position} String: {newLotProfile.PhoneNumber}"));
                RespondTo(PDU, buyPDU);
            }

            if (newLotProfile == null) // no response from server 
            {
                Failure();
                return;
            }
            Success(); // server responded with new lot
        }

        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetRoommateInfoByLotIDRequest"/>
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

            //**download roommates from data service
            if (!TryGetService(out nio2soVoltronDataServiceClient client))
                throw new NullReferenceException("nio2so client was not found.");
            IEnumerable<AvatarIDToken>? roommates = (client.GetRoommatesByHouseID(HouseID)?.Result);
            //**
            if (roommates == null) throw new NullReferenceException($"HouseID: {HouseID} was not found.");

            RespondTo(roommatePDU, new TSOGetRoommateInfoByLotIDResponse(HouseID,roommates.Select(x=>(uint)x).ToArray()));
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetLotList_Request)]
        public void GetLotList_Request(TSODBRequestWrapper PDU)
        {
            //download lot profiles from data service
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");
            var result = client.GetAllLotProfiles().Result;

            //send one packet for every lot in the world view
            foreach(var lot in result.Lots)
                RespondTo(PDU, new TSOGetLotListResponse(lot.HouseID, lot.Position));

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

            //download lot profile from data service
            LotProfile lot = GetLotProfile(req.HouseID);

            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");

            //download avatar name
            string? avatarName = client.GetAvatarNameByAvatarID(lot.OwnerAvatar).Result;

            //send requested data
            RespondWith(new TSOGetLotByID_Response(req.HouseID, lot.Name, avatarName, lot.Description, lot.Position));
            
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Warnings, RegulatorName,
                $"GetLotByID_Request: ID: {req.HouseID}"));

            TSOListOccupantsResponsePDU occupantsPDU = new TSOListOccupantsResponsePDU(new(lot.PhoneNumber,lot.Name), 
                new TSOPlayerInfoStruct(new(lot.OwnerAvatar,"bisquick")),
                new TSOPlayerInfoStruct(new(161, "FriendlyBuddy")));
            RespondWith(occupantsPDU);
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
            if (req.HouseID == 0) return;
            //**request from nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new InvalidOperationException("Tried to find the nio2so data service client, it is not present in the Services collection.");
            //**download thumbnail
            byte[] pngBytes = client.GetThumbnailByHouseID(req.HouseID).Result;
            if (pngBytes == null)
                throw new NullReferenceException(nameof(pngBytes));

            RespondWith(new TSOGetHouseThumbByIDResponse(req.HouseID, pngBytes));
        }
        /// <summary>
        /// Handles when a Client requests to update the PNG thumbnail image for a lot using <see cref="TSOSetThumbnailByIDRequest"/>
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetHouseThumbByID_Request)]
        public async void SetHouseThumbByID_Request(TSODBRequestWrapper PDU)
        {
            TSOSetThumbnailByIDRequest req = (TSOSetThumbnailByIDRequest)PDU;
            if (req.HouseID == 0) return;
            //**request the nio2so client
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new InvalidOperationException("Tried to find the nio2so data service client, it is not present in the Services collection.");
            
            byte[] pngBytes = req.PNGBytes;
            if (pngBytes == null)
                throw new NullReferenceException(nameof(pngBytes));

            //**upload thumbnail
            await client.SetHouseThumbnailByID(req.HouseID, pngBytes);
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetHouseLeaderByIDRequest"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
        public void GetHouseLeaderByLotID_Request(TSODBRequestWrapper PDU)
        {
            uint HouseID = ((TSOGetHouseLeaderByIDRequest)PDU).HouseID;

            //CHANGE LATER**********

            //download lot profiles from data service
            LotProfile lot = GetLotProfile(HouseID);
            //********

            RespondTo(PDU, new TSOGetHouseLeaderByIDResponse(HouseID, lot.OwnerAvatar));
        }
        /// <summary>
        /// This function is invoked when the <see cref="LotProtocol"/> receives an incoming <see cref="TSOGetHouseBlobByIDRequest"/>
        /// <para/>Responds with a new <see cref="TSOGetHouseBlobByIDResponse"/> with the <see cref="TSODBHouseBlob"/> requested by its <see cref="TSOGetHouseBlobByIDRequest.HouseID"/>
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
        /// <para/>Responds with nothing. Saves the new fields to the data service
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        // The Client is requesting to set the house data for this HouseID in the Database
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetLotByID_Request)]
        public void SetLotByID_Request(TSODBRequestWrapper PDU)
        { // client is giving us lot profile data
            TSOSetLotByIDRequest req = (TSOSetLotByIDRequest)PDU;

            //get nio2so client
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");
            
            //set data
            bool nameSuccessful = client.MutateLotProfileField(req.LotID, "name", req.LotName).Result.IsSuccessStatusCode;
            bool descSuccessful = client.MutateLotProfileField(req.LotID, "description", req.LotDescription).Result.IsSuccessStatusCode;

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
            uint HouseID = TestingConstraints.MyHouseID;

            //get default house
            LotProfile lot = GetLotProfile(HouseID);

            var destroyRoomPDU = (TSODestroyRoomPDU)PDU;
            LotProfile myRoom = lot;

            //Get the avatar who is trying to leave and destroy the room behind them
            uint RoomID = ((ITSONumeralStringStruct)destroyRoomPDU.RoomInfo)?.NumericID ?? 0;
            if (RoomID == 0)
                throw new InvalidDataException($"{nameof(DESTROY_ROOM_PDU)}(): {nameof(RoomID)} is {RoomID}! Ignoring...");

            RespondWith(new TSOUpdateRoomPDU(134, TSORoomInfoStruct.NoRoom));
            RespondWith(new TSODestroyRoomResponsePDU(TSOStatusReasonStruct.Online, new(myRoom.PhoneNumber,myRoom.Name)));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_PDU)]
        public void LIST_ROOMS_PDU(TSOVoltronPacket PDU)
        {
            //download all lots from data service ... change later to be rooms
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");
            var lots = client.GetAllLotProfiles().Result.Lots;

            HashSet<TSORoomInfoStruct> rooms = new();
            int i = 0;
            foreach(var ID in lots.Select(x => x.HouseID)) // UPDATE LATER TO BE ONLINE LOTS
            {
                var lot = GetLotProfile(ID);
                rooms.Add(new TSORoomInfoStruct(
                    new TSORoomIDStruct(lot.PhoneNumber, lot.Name),
                    GetAvatarIDStruct(lot.OwnerAvatar), // change this later
                    (uint)++i) 
                );
            }
            RespondWith(new TSOListRoomsResponsePDU(rooms.ToArray()));
            return;

            //test message pdu ... doesn't work
            RespondWith(new TSOAnnouncementMsgPDU(new TSOPlayerInfoStruct(new(161, "FriendlyBuddy")), "Testing"));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
        public void LOT_ENTRY_REQUEST_PDU(TSOVoltronPacket PDU)
        {
            var roomPDU = (TSOLotEntryRequestPDU)PDU;

            //identify client
            nio2soClientSessionService clientSessionService = GetService<nio2soClientSessionService>();
            if (clientSessionService.CurrentClient == null)
                throw new InvalidOperationException("Cannot identify who sent this packet to Voltron.");

            //get the lot
            LotProfile thisLot = GetLotProfile(roomPDU.HouseID);
            bool hosting = false;
            uint joiningAvatarID = ((ITSONumeralStringStruct)clientSessionService.CurrentClient).NumericID ?? 0;
            if (joiningAvatarID == 0)
                throw new Exception("Joining client is not identified. AvatarID: " + joiningAvatarID);

            //get the phone number of the lot
            string phoneNumber = thisLot?.PhoneNumber ?? TestingConstraints.MyHousePhoneNumber;

            //Update which room they're in currently
            string LotName = thisLot?.Name ?? "BloatyLand";
            string RoomName = LotName;

            //**join failed
            bool successfulJoin = true;
            if (!successfulJoin)
            {
                uint errorCode = 0;
                RespondWith(new TSOJoinRoomFailedPDU(10, "", new(phoneNumber, RoomName)));
                return;
            }

            if (true)//thisLot.OwnerAvatar == ((ITSONumeralStringStruct)clientSessionService.CurrentClient).NumericID)
            { // Initiate the host protocol
                // TELL THE CLIENT TO START THE HOST PROTOCOL
                RespondWith(new TSOHouseSimConstraintsResponsePDU(roomPDU.HouseID));
                hosting = true;
                // set lot as ONLINE
                _onlineLotIDs.Add(thisLot.HouseID);
            }                              

            //create a list of admins
            var admins = new TSOAriesIDStruct[] { new TSOAriesIDStruct(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName) };
            uint occupants = (uint)admins.Length;

            //create the roominfo struct
            TSORoomInfoStruct roomInfo = new TSORoomInfoStruct(roomLocationInfo: new(phoneNumber, RoomName),
                ownerVector: GetAvatarIDStruct(thisLot.OwnerAvatar.AvatarID), // m_ownerID
                currentOccupants: occupants, maxOccupants: 10, isLocked: false,
                AdminList: admins
            );            

            //**join succeeded
            //tell the client to join this new room
            TSOJoinRoomPDU joinPDU = new TSOJoinRoomPDU(roomInfo.RoomLocationInfo, "bloatytime3!");
            RespondWith(joinPDU);

            TSOUpdateRoomPDU updateRoomPDU = new TSOUpdateRoomPDU(0, roomInfo, true);
            RespondWith(updateRoomPDU);

            TSOListOccupantsResponsePDU occupantsPDU = new TSOListOccupantsResponsePDU(roomInfo.RoomLocationInfo, new TSOPlayerInfoStruct(admins[0]));
            RespondWith(occupantsPDU);
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

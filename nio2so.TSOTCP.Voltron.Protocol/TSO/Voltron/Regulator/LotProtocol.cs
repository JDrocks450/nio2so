﻿using nio2so.Data.Common.Testing;
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

        /// <summary>
        /// Downloads the <see cref="LotProfile"/> for the given <paramref name="HouseID"/> using the <see cref="nio2soVoltronDataServiceClient"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public LotProfile GetLotProfile(HouseIDToken HouseID)
        {
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");
            LotProfile? item = client.GetLotProfileByHouseID(HouseID).Result;
            if (item == null) throw new InvalidDataException($"LotID {HouseID} doesn't exist.");
            return item;
        }
        /// <summary>
        /// <inheritdoc cref="AvatarProtocol.GetAvatarIDStruct(AvatarIDToken)"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public TSOAriesIDStruct GetVoltronIDStruct(AvatarIDToken AvatarID) => GetRegulator<AvatarProtocol>().GetVoltronIDStruct(AvatarID);

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

            //use room protocol to list the people in the lot
            TSOListOccupantsResponsePDU? occupantsPDU = GetRegulator<RoomProtocol>().GetOccupantsPDUByRoomID(req.HouseID, out bool IsOnline);            
            RespondWith(occupantsPDU);

            //send requested data
            RespondWith(new TSOGetLotByID_Response(req.HouseID, lot.Name, avatarName, lot.Description, lot.Position));
            
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Warnings, RegulatorName,
                $"GetLotByID_Request: ID: {req.HouseID}"));            
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
    }
}

using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats.DB;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using System.Reflection.Metadata;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator("AvatarProtocol")]
    internal class AvatarProtocol : TSOProtocol
    {
        /// <summary>
        /// Handles a <see cref="TSOGetCharByIDRequest"/> PDU and returns the <see cref="TSODBChar"/> data item for this player using the <see cref="nio2soVoltronDataServiceClient"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="InvalidDataException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Request)]
        public void GetCharByID_Request(TSODBRequestWrapper PDU)
        {
            var charPacket = (TSOGetCharByIDRequest)PDU;
            var avatarID = charPacket.AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException($"{TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request} AvatarID: {avatarID}. ERROR!!!");

            //**download the requested avatarID character data from nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //get tsodbchar object
            TSODBChar? charData = dataServiceClient.GetCharacterFileByAvatarID(avatarID).Result;      
            
            if (charData == null)
                throw new NullReferenceException($"{avatarID} not found in nio2so data service");

            RespondTo(PDU, new TSOGetCharByIDResponse(avatarID, charData));
        }
        /// <summary>
        /// Handles a <see cref="TSOGetCharBlobByIDRequest"/> PDU and returns the <see cref="TSODBCharBlob"/> data item for this player using the <see cref="nio2soVoltronDataServiceClient"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Request)]
        public void GetCharBlobByID_Request(TSODBRequestWrapper PDU)
        {
            var charBlobReqPDU = (TSOGetCharBlobByIDRequest)PDU;
            var avatarID = charBlobReqPDU.AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException(nameof(GetCharBlobByID_Request) + $"() AvatarID submitted was {avatarID}!");

            //**download the requested avatarID appearance data from nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //get charblob stream
            byte[]? result = dataServiceClient.GetAvatarCharBlobByAvatarID(avatarID).Result;

            if (result == null)
                throw new InvalidDataException("nio2so Data Service returned an empty result!");

            TSODBCharBlob charBlob = TSOVoltronSerializer.Deserialize<TSODBCharBlob>(result);
            if (charBlob == null)
                throw new NullReferenceException($"CharBlob {avatarID} is null and unhandled.");
            if (charBlob.AvatarID != avatarID) // oh no that's not good.
            {
                if (charBlob.AvatarID == 0x1111FFFF) // oh phew this character never got a ID bestowed upon it
                    charBlob.AvatarID = avatarID;
                else throw new InvalidDataException($"AvatarID: {avatarID} requested, got {charBlob.AvatarID}'s data.");
            }
            TSOServerTelemetryServer.Global.OnCharBlob(NetworkTrafficDirections.OUTBOUND, avatarID, charBlob);
            TSOGetCharBlobByIDResponse response = new TSOGetCharBlobByIDResponse(avatarID, charBlob);
            RespondTo(PDU, response);
        }

        /// <summary>
        /// Handles an incoming <see cref="TSOGetBookmarksRequest"/> and returns the player's bookmarks, which are downloaded from the <see cref="nio2soVoltronDataServiceClient"/>
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Request)]
        public void GetBookmarks_Request(TSODBRequestWrapper PDU)
        {
            var bookmarkPDU = (TSOGetBookmarksRequest)PDU;
            var avatarID = bookmarkPDU.AvatarID;
            if (avatarID == 0) return;

            //**download bookmarks from nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //get result from nio2so data service
            DataService.Common.Queries.N2BookmarksByAvatarIDQueryResult? result = dataServiceClient.GetAvatarBookmarksByAvatarID(avatarID).Result;

            uint[] avatarBookmarks;
            if (result == null) // server error
                avatarBookmarks = new uint[0]; // send empty response on server error
            else avatarBookmarks = result.Avatars.Select(x => (uint)x).ToArray(); // populate bookmarks
            
            RespondTo(PDU, new TSOGetBookmarksResponse(avatarID, avatarBookmarks)); // Add more to test Bookmarks
        }

        /// <summary>
        /// Handles an incoming <see cref="TSOSetBookmarksListRequest"/> and uploads the bookmark list using the <see cref="nio2soVoltronDataServiceClient"/>
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetBookmarks_Request)]
        public void SetBookmarks_Request(TSODBRequestWrapper PDU)
        {
            TSOSetBookmarksListRequest req = (TSOSetBookmarksListRequest)PDU;
            if (!req.Bookmarks.Any()) return;
            //**upload bookmarks to nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //get result from nio2so data service
            bool result = dataServiceClient.SetAvatarBookmarksByAvatarID(req.AvatarID, req.Bookmarks.Select(x => (AvatarIDToken)x).ToArray()).Result;
        }

        /// <summary>
        /// The client is attempting to send us a newly created <see cref="TSODBCharBlob"/> object.
        /// We will take the new data and add it to our pseudo-database for later usage
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="InvalidDataException"></exception>        
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request)]
        public void InsertNewCharBlob_Request(TSODBRequestWrapper PDU)
        {
            //Data1 is the avatarID -- this is given by the Shard-Selector servlet
            var avatarID = ((TSOInsertCharBlobByIDRequest)PDU).AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException("You cannot provide zero as an AvatarID when sending a Client to CAS. " +
                    "Give the Client an actual AvatarID. Ignoring this PDU.");

            //decompress enclosed stream into a TSODBCharBlob
            if (!((TSOInsertCharBlobByIDRequest)PDU).TryUnpack(out TSODBCharBlob? Blob) || Blob == null)
                throw new InvalidDataException("Could not decompress this PDU into a TSODBCharBlob!!");
            if (Blob.AvatarID != avatarID)
                Blob.AvatarID = avatarID;

            //**upload the new avatar appearance data to nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //just upload the TSOCharBlob to nio2so
            bool result = dataServiceClient.SetAvatarCharBlobByAvatarID(avatarID, TSOVoltronSerializer.Serialize(Blob)).Result;

            if (!result)
                throw new InvalidOperationException("nio2so Data Service refused the changes.");

            //log this to disk
            TSOServerTelemetryServer.Global.OnCharBlob(NetworkTrafficDirections.INBOUND, avatarID, Blob);
            RespondTo(PDU, new TSOInsertCharBlobByIDResponse(avatarID));
        }

        /// <summary>
        /// The client is attempting to send us a modified <see cref="TSODBCharBlob"/> object.
        /// We will take the new data and upload it to the nio2so data service
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="InvalidDataException"></exception>        
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Request)]
        public void SetCharBlob_Request(TSODBRequestWrapper PDU)
        {
            var avatarID = ((TSOSetCharBlobByIDRequest)PDU).AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException("Client provided AvatarID: 0 as the one to update which is not valid. Ignoring...");

            //decompress enclosed stream into a TSODBCharBlob
            if (!((TSOSetCharBlobByIDRequest)PDU).TryUnpack(out TSODBCharBlob? Blob) || Blob == null)
                throw new InvalidDataException("Could not decompress this PDU into a TSODBCharBlob!!");

            if (Blob.AvatarID != avatarID)
                throw new InvalidDataException("Submitted Character Blob from the server doesn't match the avatar we're modifying! Changes not saved...");

            //**upload the new appearance data to nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //just upload the TSOCharBlob to nio2so
            bool result = dataServiceClient.SetAvatarCharBlobByAvatarID(avatarID, TSOVoltronSerializer.Serialize(Blob)).Result;

            if (!result)
                throw new InvalidOperationException("nio2so Data Service refused the changes.");

            //log this to disk
            TSOServerTelemetryServer.Global.OnCharBlob(NetworkTrafficDirections.INBOUND, avatarID, Blob);
            RespondTo(PDU, new TSOSetCharBlobByIDResponse(avatarID, Blob));
        }

        /// <summary>
        /// The client is attempting to send us a modified <see cref="TSODBChar"/> object.
        /// We will take the new data and add it to our pseudo-database for later usage
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="InvalidDataException"></exception>    
        /// <exception cref="InvalidDataException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request)]
        public void SetCharByID_Request(TSODBRequestWrapper PDU)
        {
            var charPacket = (TSOSetCharByIDRequest)PDU;
            var avatarID = charPacket.AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException($"{TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request} AvatarID: {avatarID}. ERROR!!!");

            TSODBChar charData = charPacket.CharProfile;

            //**upload the new appearance data to nio2so
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            //just upload the TSOCharBlob to nio2so
            bool result = dataServiceClient.SetCharacterFileByAvatarID(avatarID, charData).Result;

            //log this to disk
            TSOServerTelemetryServer.Global.OnCharData(NetworkTrafficDirections.INBOUND, avatarID, charData);
            RespondTo(PDU, new TSOSetCharByIDResponse(avatarID));
        }
        /// <summary>
        /// Invoked when the AvatarID embedded in the PDU should add/remove funds from their account
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Request)]
        public void DebitCredit_Request(TSODBRequestWrapper PDU)
        {
            var debitcreditPDU = (TSODebitCreditRequestPDU)PDU;
            var avatarID = debitcreditPDU.AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException($"{TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request} AvatarID: {avatarID}. ERROR!!!");

            //log this to disk
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                RegulatorName, $"AvatarID: {debitcreditPDU.AvatarID} Account: {debitcreditPDU.Account} +/-: {debitcreditPDU.Amount}"));

            RespondTo(PDU, new TSODebitCreditResponsePDU(
                debitcreditPDU.AvatarID, debitcreditPDU.Account, debitcreditPDU.Amount));
        }

        /// <summary>
        /// Unsure what this does. Sent whenever a money transaction happens.
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="InvalidDataException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SetMoneyFields_Request)]
        public void SetMoneyFields_Request(TSODBRequestWrapper PDU)
        {
            var moneyPacket = (TSOSetMoneyFieldsRequest)PDU;
            var avatarID = moneyPacket.AvatarID;
            if (avatarID == 0)
                throw new InvalidDataException($"{TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request} AvatarID: {avatarID}. ERROR!!!");
            
            RespondTo(PDU, new TSOSetMoneyFieldsResponse(avatarID, 1, 2, 3));
        }

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Request)]
        public void GetRelationshipsByID_Request(TSODBRequestWrapper PDU)
        {
            var relPDU = (TSOGetRelationshipsByIDRequest)PDU;
            uint avatarID = relPDU.AvatarID;
            RespondTo(PDU, new TSOGetRelationshipsByIDResponse(avatarID, TestingConstraints.MyFriendAvatarID));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU)]
        public void FIND_PLAYER_PDU(TSOVoltronPacket PDU)
        {
            var formattedPacket = (TSOFindPlayerPDU)PDU;
            if (!string.IsNullOrWhiteSpace(formattedPacket?.RequestedPlayer?.AriesID))
            {
                if (uint.TryParse(formattedPacket.RequestedPlayer.AriesID.Substring(
                    formattedPacket.RequestedPlayer.AriesID.IndexOf("A ") + 2), out uint AvatarID))
                {
                    formattedPacket.RequestedPlayer.MasterID = TestingConstraints.MyFriendAvatarName;
                    RespondWith(new TSOFindPlayerResponsePDU(new(formattedPacket.RequestedPlayer.AriesID, formattedPacket.RequestedPlayer.MasterID)));
                    TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                        RegulatorName, $"FIND PLAYER: {AvatarID}"));
                    return;
                }
                TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                        RegulatorName, $"FIND PLAYER: ERROR! {formattedPacket.RequestedPlayer.AriesID} ???"));
                return;
            }
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                        RegulatorName, $"FIND PLAYER: ERROR! ARIES ID WAS NULL!"));
        }

        public override bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            //This is all other PDUs not yet implemented

            bool success = false;

            void statusUpdated(TSOVoltronPacket statusUpdater)
            {
                TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                    RegulatorName, $"[STATUS_UPDATED] {statusUpdater}"));
            }

            switch (PDU.KnownPacketType)
            {
                // Sent when a player creates a new Avatar from CAS
                case TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_CREATEAVATARNOTIFICATION_PDU:
                    {
                        TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                            RegulatorName, $"({TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_CREATEAVATARNOTIFICATION_PDU}) Avatar was created."));
                    }
                    success = true;
                    break;
                //**STATUS UPDATERS**
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU:
                    statusUpdated(new TSOSetAcceptAlertsResponsePDU(((TSOSetAcceptAlertsPDU)PDU).AcceptingAlerts == 0)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU:
                    statusUpdated(new TSOSetAcceptFlashesResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU:
                    statusUpdated(new TSOSetIgnoreListResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU:
                    statusUpdated(new TSOSetInvincibleResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU:
                    statusUpdated(new TSOSetInvisibleResponsePDU(false)); break;
            }
            Response = null;
            if (success)
                return success;
            return base.HandleIncomingPDU(PDU, out Response);
        }
    }
}

﻿using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator("AvatarProtocol")]
    internal class AvatarProtocol : TSOProtocol
    {
        /// <summary>
        /// Handles a <see cref="TSOGetCharByIDRequest"/> PDU and returns the <see cref="TSODBChar"/> data item for this player using the <see cref="TSOAvatarFactory"/>
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

            TSODBChar charData = TSOFactoryBase.Get<TSOAvatarFactory>().GetCharByID(avatarID);
            charData.Funds = TestingConstraints.StaticFunds; // set beta funds level here
            RespondTo(PDU, new TSOGetCharByIDResponse(avatarID, charData));
        }
        /// <summary>
        /// Handles a <see cref="TSOGetCharBlobByIDRequest"/> PDU and returns the <see cref="TSODBCharBlob"/> data item for this player using the <see cref="TSOAvatarFactory"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Request)]
        public void GetCharBlobByID_Request(TSODBRequestWrapper PDU)
        {
            var charBlobReqPDU = (TSOGetCharBlobByIDRequest)PDU;
            var avatarID = charBlobReqPDU.AvatarID;
            if (avatarID == 0) avatarID = 1337;
            TSODBCharBlob charBlob = TSOFactoryBase.Get<TSOAvatarFactory>().GetCharBlobByID(avatarID);
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
        /// Handles an incoming <see cref="TSOGetBookmarksRequest"/> and returns the player's bookmarks, which are hard-coded right now
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Request)]
        public void GetBookmarks_Request(TSODBRequestWrapper PDU)
        {
            var bookmarkPDU = (TSOGetBookmarksRequest)PDU;
            var avatarID = bookmarkPDU.AvatarID;
            if (avatarID == 0) return;

            RespondTo(PDU, new TSOGetBookmarksResponse(avatarID,
                                                      TestingConstraints.MyFriendAvatarID,
                                                      TestingConstraints.MyAvatarID)); // Add more to test Bookmarks
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

            //log this to disk
            TSOServerTelemetryServer.Global.OnCharBlob(NetworkTrafficDirections.INBOUND, avatarID, Blob);
            RespondTo(PDU, new TSOInsertCharBlobByIDResponse(avatarID));
        }
        /// <summary>
        /// The client is attempting to send us a modified <see cref="TSODBCharBlob"/> object.
        /// We will take the new data and add it to our pseudo-database for later usage
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

            void logme(TSOVoltronPacket statusUpdater)
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
                    logme(new TSOSetAcceptAlertsResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU:
                    logme(new TSOSetAcceptFlashesResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU:
                    logme(new TSOSetIgnoreListResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU:
                    logme(new TSOSetInvincibleResponsePDU(false)); break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU:
                    logme(new TSOSetInvisibleResponsePDU(false)); break;
            }
            Response = null;
            if (success)
                return success;
            return base.HandleIncomingPDU(PDU, out Response);
        }
    }
}

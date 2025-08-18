using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    [TSORegulator]
    /// <summary>
    /// This protocol handles lower-level functions of the Voltron Data Service such as 
    /// <see cref="TSOHostOnlinePDU"/>, <see cref="TSOClientBye"/>
    /// </summary>
    internal class VoltronDMSProtocol : TSOProtocol, IDMSProtocol
    {
        [TSOProtocolDatabaseHandler((uint)TSO_PreAlpha_DBActionCLSIDs.InsertGenericLog_Request)]
        public void InsertGenericLog_Request(TSODBRequestWrapper PDU)
        {
            var logPDU = (TSOInsertGenericLogRequest)PDU;
            string message = logPDU.ConsoleLog;
            LogConsole($"ServerLog: Type: {(TSO_PreAlpha_GZPROBE)logPDU.ProbeCLSID} {message}", "cDBServiceClientD");
        }

        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.BYE_PDU)]
        public void BYE_PDU(TSOVoltronPacket PDU)
        {
            var bye_pdu = (TSOClientBye)PDU;
            LogConsole("Client is saying Bye! Disconnecting after frame...");
            RespondWith(bye_pdu);
        }
        /// <summary>
        /// A TSOClient is logging into Voltron
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU)]
        public void CLIENT_ONLINE_PDU(TSOVoltronPacket PDU)
        {
            if (GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID))
            {
                //PURGE PREVIOUS SESSION IF NOT CAUGHT BY ON_DISCONNECT
                GetRegulator<RoomProtocol>().AvatarPurgePlaySession(VoltronID, out string error);
                LogConsole($"AvatarPurgePreviousSession(): AvatarID: {VoltronID.AvatarID}:" + error);
                //SET ONLINE STATUS TO TRUE
                if(GetDataService().SetOnlineStatusByAvatarID(VoltronID.AvatarID, true).Result.IsSuccessStatusCode) 
                    LogConsole($"SetAvatarOnlineStatus(): AvatarID: {VoltronID.AvatarID} Value: {true}");
            }
            return;
            /*
            uint avatarID = TSOVoltronConst.MyAvatarID;
            string avatarName = TSOVoltronConst.MyAvatarName;
            RespondWith(new TSOUpdatePlayerPDU(new Struct.TSOAriesIDStruct(avatarID, avatarName)));*/
        }
        /// <summary>
        /// Logic to run when a TSOClient disconnects from Voltron
        /// </summary>
        public void ON_DISCONNECT(uint QuazarID)
        {
            nio2soClientSessionService clientSession = GetService<nio2soClientSessionService>();
            if(clientSession.RemoveClient(QuazarID, out TSOAriesIDStruct? VoltronID))
            {
                //SET ONLINE STATUS TO FALSE
                if (GetDataService().SetOnlineStatusByAvatarID(VoltronID.AvatarID, true).Result.IsSuccessStatusCode)
                    LogConsole($"SetAvatarOnlineStatus(): AvatarID: {VoltronID.AvatarID} Value: {false}");
                LogConsole($"{nameof(ON_DISCONNECT)}(): AvatarID: {VoltronID.AvatarID} is leaving Voltron... bye-bye!", nameof(ON_DISCONNECT), TSOLoggerServiceBase.LogSeverity.Warnings);
                // CLEAN THIS CLIENT OUT OF ANY ROOMS THEY'RE IN
                if (GetRegulator<RoomProtocol>().AvatarPurgePlaySession(VoltronID, out string error))
                    LogConsole($"AvatarPurgePreviousSession(): AvatarID: {VoltronID.AvatarID}:" + error);
            }
            else LogConsole($"{nameof(ON_DISCONNECT)}(): QuaZarID: {QuazarID}(NO VOLTRON_ID!!) is leaving Voltron... cya!", nameof(ON_DISCONNECT), TSOLoggerServiceBase.LogSeverity.Errors);
        }

        public TSOVoltronPacket GET_UPDATE_PLAYER(uint AvatarID, out string AvatarName)
        {
            TSOPlayerInfoStruct playerInfo = GetRegulator<AvatarProtocol>().GetPlayerInfoStruct(AvatarID);
            AvatarName = playerInfo.PlayerID.MasterID;
            return new TSOUpdatePlayerPDU(playerInfo);
        }

        public TSOVoltronPacket GET_HOST_ONLINE(ushort ClientBufferLength, params string[] Badwords) => new TSOHostOnlinePDU(ClientBufferLength, Badwords);


        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
        public void BC_VERSION_LIST_PDU(TSOVoltronPacket PDU)
        {
            TSOBCVersionListPDU pdu = (TSOBCVersionListPDU)PDU;
            RespondWith(new TSOBCVersionListPDU(pdu.VersionString, "", 0x01));
        }
    }
}

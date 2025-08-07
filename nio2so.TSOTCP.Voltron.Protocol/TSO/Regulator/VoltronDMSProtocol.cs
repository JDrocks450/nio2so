using nio2so.TSOTCP.Voltron.Protocol.Services;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Regulator
{
    [TSORegulator]
    /// <summary>
    /// This protocol handles lower-level functions of the Voltron Data Service such as 
    /// <see cref="TSOHostOnlinePDU"/>, <see cref="TSOClientBye"/>
    /// </summary>
    internal class VoltronDMSProtocol : TSOProtocol
    {
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.InsertGenericLog_Request)]
        public void InsertGenericLog_Request(TSODBRequestWrapper PDU)
        {
            var logPDU = (TSOInsertGenericLogRequest)PDU;
            string message = logPDU.ConsoleLog;
            TSOServerTelemetryServer.Global.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Warnings,
                "cDBServiceClientD", $"ServerLog: Type: {(TSO_PreAlpha_GZPROBE)logPDU.ProbeCLSID} {message}"));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.BYE_PDU)]
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
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU)]
        public void CLIENT_ONLINE_PDU(TSOVoltronPacket PDU)
        {
            if (GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out Struct.TSOAriesIDStruct? VoltronID))
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
        internal void ON_DISCONNECT(uint QuazarID)
        {
            nio2soClientSessionService clientSession = GetService<nio2soClientSessionService>();
            if(clientSession.RemoveClient(QuazarID, out Struct.TSOAriesIDStruct? VoltronID))
            {
                //SET ONLINE STATUS TO FALSE
                if (GetDataService().SetOnlineStatusByAvatarID(VoltronID.AvatarID, true).Result.IsSuccessStatusCode)
                    LogConsole($"SetAvatarOnlineStatus(): AvatarID: {VoltronID.AvatarID} Value: {false}");
                LogConsole($"{nameof(ON_DISCONNECT)}(): AvatarID: {VoltronID.AvatarID} is leaving Voltron... bye-bye!", nameof(ON_DISCONNECT), TSOServerTelemetryServer.LogSeverity.Warnings);
                // CLEAN THIS CLIENT OUT OF ANY ROOMS THEY'RE IN
                if (GetRegulator<RoomProtocol>().AvatarPurgePlaySession(VoltronID, out string error))
                    LogConsole($"AvatarPurgePreviousSession(): AvatarID: {VoltronID.AvatarID}:" + error);
            }
            else LogConsole($"{nameof(ON_DISCONNECT)}(): QuaZarID: {QuazarID}(NO VOLTRON_ID!!) is leaving Voltron... cya!", nameof(ON_DISCONNECT), TSOServerTelemetryServer.LogSeverity.Errors);
        }

        internal TSOVoltronPacket GetUpdatePlayerPDU(uint AvatarID, out string AvatarName)
        {
            Struct.TSOPlayerInfoStruct playerInfo = GetRegulator<AvatarProtocol>().GetPlayerInfoStruct(AvatarID);
            AvatarName = playerInfo.PlayerID.MasterID;
            return new TSOUpdatePlayerPDU(playerInfo);
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
        public void BC_VERSION_LIST_PDU(TSOVoltronPacket PDU)
        {
            TSOBCVersionListPDU pdu = (TSOBCVersionListPDU)PDU;
            RespondWith(new TSOBCVersionListPDU(pdu.VersionString, "", 0x01));
        }
    }
}

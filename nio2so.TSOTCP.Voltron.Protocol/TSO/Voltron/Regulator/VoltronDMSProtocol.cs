using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.TSOTCP.Voltron.Protocol.Services;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    [TSORegulator]
    /// <summary>
    /// This protocol handles lower-level functions of the Voltron Data Service such as 
    /// <see cref="TSOHostOnlinePDU"/>, <see cref="TSOClientByePDU"/>
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
            LogMessage("Client is saying Bye! Disconnecting after frame...");
            RespondWith(bye_pdu);
        }
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU)]
        public void CLIENT_ONLINE_PDU(TSOVoltronPacket PDU)
        {
            return;
            uint avatarID = TSOVoltronConst.MyAvatarID;
            string avatarName = TSOVoltronConst.MyAvatarName;
            RespondWith(new TSOUpdatePlayerPDU(new Struct.TSOAriesIDStruct(avatarID, avatarName)));
        }

        internal TSOVoltronPacket VOLTRON_DMS_GetUpdatePlayerPDU(uint AvatarID, out string AvatarName)
        {
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new Exception("nio2so data service client could not be connected.");
            AvatarProfile? name = client.GetAvatarProfileByAvatarID(AvatarID).Result;
            if (name == null)
                throw new NullReferenceException("nio2so could not find the requested AvatarName by ID.");
            AvatarName = name.Name;
            return new TSOUpdatePlayerPDU(new Struct.TSOAriesIDStruct(AvatarID,AvatarName));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
        public void BC_VERSION_LIST_PDU(TSOVoltronPacket PDU)
        {
            TSOBCVersionListPDU pdu = (TSOBCVersionListPDU)PDU;
            RespondWith(new TSOBCVersionListPDU(pdu.VersionString, "", 0x01));
        }

        private void LogMessage(string Message)
        {
            TSOServerTelemetryServer.Global.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Message,
                RegulatorName, Message));
        }
    }
}

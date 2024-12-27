using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.HSBServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    [Voltron.Regulator.TSORegulator("VoltronDMSProtocol")]
    /// <summary>
    /// This protocol handles lower-level functions of the Voltron Data Service such as 
    /// <see cref="TSOHostOnlinePDU"/>, <see cref="TSOClientByePDU"/>
    /// </summary>
    internal class VoltronDMSProtocol : TSOProtocol
    {
        public string RegulatorName => "VoltronDMSProtocol";

        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.InsertGenericLog_Request)]
        public void InsertGenericLog_Request(TSODBRequestWrapper PDU)
        {
            var logPDU = (TSOInsertGenericLogRequest)PDU;
            string message = logPDU.ConsoleLog;
            TSOServerTelemetryServer.Global.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Warnings,
                "cDBServiceClientD", $"ServerLog: Type: {(TSO_PreAlpha_GZPROBE)logPDU.ProbeCLSID} {message}"));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.BYE_PDU)]
        public void BYE_PDU(TSOVoltronPacket PDU) {
            var bye_pdu = (TSOClientBye)PDU;
            LogMessage("Client is saying Bye! Disconnecting after frame...");
            RespondWith(new TSOClientBye(bye_pdu.StatusCode, bye_pdu.Message));
        }
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU)]
        public void CLIENT_ONLINE_PDU(TSOVoltronPacket PDU)
        {
            uint avatarID = TSOVoltronConst.MyAvatarID;
            string avatarName = TSOVoltronConst.MyAvatarName;
            if (Server == HSBSession.RoomServer)
            {
                avatarID = TestingConstraints.HSBHostID;
                avatarName = TestingConstraints.HSBHostName;
            }
            RespondWith(new TSOUpdatePlayerPDU(avatarID,avatarName));
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

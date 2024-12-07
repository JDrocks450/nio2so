using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
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
            TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Warnings,
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
            RespondWith(new TSOUpdatePlayerPDU(TSOVoltronConst.MyAvatarID, TSOVoltronConst.MyAvatarName));
        }
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
        public void BC_VERSION_LIST_PDU(TSOVoltronPacket PDU)
        {
            TSOBCVersionListPDU pdu = (TSOBCVersionListPDU)PDU;
            RespondWith(new TSOBCVersionListPDU(pdu.VersionString, "", 0x01));
        }

        private void LogMessage(string Message)
        {
            TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message,
                RegulatorName, Message));
        }
    }
}

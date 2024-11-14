using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.Collections;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    [TSORegulator("cTSOSplitBufferProtocol")]
    internal class SplitBufferPDURegulator : ITSOProtocolRegulator
    {
        public string RegulatorName => "cTSOSplitBufferProtocol";

        private TSOSplitBufferPDUCollection _SplitBufferPDUs = new();

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            Response = default;
            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            var ResponsePackets = new List<TSOVoltronPacket>();
            var Insertionpackets = new List<TSOVoltronPacket>();
            Response = new(ResponsePackets, Insertionpackets, null);

            if (PDU is TSOSplitBufferPDU splitBuffer)
            {
                _SplitBufferPDUs.Add(splitBuffer);
                if (!splitBuffer.HasDataRemaining)
                { // all packets received. dispose and reset
                    var enclosedPDU = TSOPDUFactory.CreatePacketObjectFromSplitBuffers(_SplitBufferPDUs);
                    Insertionpackets.Add(enclosedPDU);
                    _SplitBufferPDUs.Dispose();
                    _SplitBufferPDUs = new();
                    TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message,
                        RegulatorName, $"Inserted the {enclosedPDU}"));
                }
                return true;
            }
            return false;
        }
    }
}

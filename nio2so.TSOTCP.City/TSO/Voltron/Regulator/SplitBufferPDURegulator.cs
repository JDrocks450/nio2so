using nio2so.TSOTCP.City.Factory;
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
        /// <summary>
        /// When merging a split_buffer_pdu this stores what packet we're currently unpacking
        /// </summary>
        private TSOVoltronPacketHeader? _VoltronPacketHeader;
        private uint _recvBytes = 0;
        public bool IsUnpacking => _VoltronPacketHeader != null;

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
                if (!IsUnpacking)
                { // START UNPACKING                    
                    _VoltronPacketHeader = TSOVoltronPacket.ReadVoltronHeader(splitBuffer.DataBuffer);
                    _recvBytes = 0;
                }
                _recvBytes += splitBuffer.SplitBufferPayloadSize;
                _SplitBufferPDUs.Add(splitBuffer);
                if ((_recvBytes >= _VoltronPacketHeader.PDUPayloadSize) || !splitBuffer.HasDataRemaining)
                { // all packets received. dispose and reset
                    var enclosedPDU = TSOPDUFactory.CreatePacketObjectFromSplitBuffers(_SplitBufferPDUs);
                    Insertionpackets.Add(enclosedPDU);

                    _SplitBufferPDUs.Dispose();
                    _SplitBufferPDUs = new();
                    _VoltronPacketHeader = null;
                    _recvBytes = 0;

                    TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message,
                        RegulatorName, $"Inserted the {enclosedPDU}"));
                }
                return true;
            }
            return false;
        }
    }
}

﻿using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Collections;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming <see cref="TSOSplitBufferPDU"/> PDUs from a remote connection
    /// </summary>
    [TSORegulator("cTSOSplitBufferProtocol")]
    internal class SplitBufferPDUProtocol : TSOProtocol
    {
        private TSOSplitBufferPDUCollection _SplitBufferPDUs = new();
        /// <summary>
        /// When merging a <see cref="TSOSplitBufferPDU"/> this stores what packet we're currently unpacking
        /// </summary>
        private TSOVoltronPacketHeader? _VoltronPacketHeader;
        private uint _recvBytes = 0;
        private int _recvPDUs = 0;
        public bool IsUnpacking => _VoltronPacketHeader != null;

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.SPLIT_BUFFER_PDU)]
        public void DoProtocol(TSOVoltronPacket PDU)
        {
            _recvPDUs++;
            var splitBuffer = (TSOSplitBufferPDU)PDU;
            if (!IsUnpacking)
            { // START UNPACKING                    
                _VoltronPacketHeader = TSOVoltronPacket.ReadVoltronHeader(splitBuffer.DataBuffer);
                _recvBytes = 0;
            }
            _recvBytes += splitBuffer.SplitBufferPayloadSize;            
            _SplitBufferPDUs.Add(splitBuffer);

            if (_recvBytes >= _VoltronPacketHeader.PDUPayloadSize || splitBuffer.EOF)
            { // all packets received. dispose and reset
                var enclosedPDU = TSOPDUFactory.CreatePacketObjectFromSplitBuffers(_SplitBufferPDUs);
                InsertOne(enclosedPDU);

                TSOServerTelemetryServer.Global.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Message,
                    RegulatorName, $"Inserted the {enclosedPDU}\n\nFrom {_recvPDUs} {nameof(TSOSplitBufferPDU)}s ... ({_recvBytes} bytes)"));

                _SplitBufferPDUs.Dispose();
                _SplitBufferPDUs = new();
                _VoltronPacketHeader = null;
                _recvBytes = 0;
                _recvPDUs = 0;                
            }
        }
    }
}

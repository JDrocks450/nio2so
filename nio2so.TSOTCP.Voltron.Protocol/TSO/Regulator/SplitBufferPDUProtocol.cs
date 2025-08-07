using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Collections;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU;
using System.Collections.Concurrent;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Regulator
{
    /// <summary>
    /// Handles incoming <see cref="TSOSplitBufferPDU"/> PDUs from a remote connection
    /// </summary>
    [TSORegulator("cTSOSplitBufferProtocol")]
    internal class SplitBufferPDUProtocol : TSOProtocol
    {
        internal class SplitBufferPDUThreadContext : IDisposable
        {
            private TSOSplitBufferPDUCollection _SplitBufferPDUs = new();
            /// <summary>
            /// When merging a <see cref="TSOSplitBufferPDU"/> this stores what packet we're currently unpacking
            /// </summary>
            private TSOVoltronPacketHeader? _VoltronPacketHeader;
            public uint _recvBytes = 0;
            public int _recvPDUs = 0;
            public bool IsUnpacking => _VoltronPacketHeader != null;

            internal void DoProtocolOnThread(TSOVoltronPacket PDU, out TSOVoltronPacket? UnsplitPacket)
            {
                UnsplitPacket = null;

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
                    UnsplitPacket = TSOPDUFactory.CreatePacketObjectFromSplitBuffers(_SplitBufferPDUs);             
                    //remember to dispose later :) !
                }
            }

            public void Dispose()
            {
                _SplitBufferPDUs?.Dispose();
                _SplitBufferPDUs = new();
                _VoltronPacketHeader = null;
                _recvBytes = 0;
                _recvPDUs = 0;
            }
        }

        private readonly ConcurrentDictionary<int, SplitBufferPDUThreadContext> _threads = new();

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.SPLIT_BUFFER_PDU)]
        public void DoProtocol(TSOVoltronPacket PDU)
        {
            int ID = Thread.CurrentThread.ManagedThreadId;
            void CreateContext(int ThreadID)
            {
                _threads.TryAdd(ThreadID, new());
            }
            if (!_threads.ContainsKey(ID))
                CreateContext(ID);
            if (!_threads.TryGetValue(ID, out SplitBufferPDUThreadContext? context) || context == null)
                throw new Exception($"{nameof(TSOSplitBufferPDU)} cannot create a new context for the thread: {ID}");
            context.DoProtocolOnThread(PDU, out TSOVoltronPacket? DesplitPDU);
            if (DesplitPDU != null)
            { // decompressed a PDU ... insert it into this voltron aries frame
                InsertOne(DesplitPDU);

                TSOServerTelemetryServer.Global.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Message,
                    RegulatorName, $"Inserted the {DesplitPDU}\n\nFrom {context._recvPDUs} {nameof(TSOSplitBufferPDU)}s ... ({context._recvBytes} bytes)"));

                context.Dispose();
                _threads.TryRemove(ID, out _);
            }
        }
    }
}

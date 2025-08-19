using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Collections;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.PlayTest.Protocol.PDU;
using System.Collections.Concurrent;

namespace nio2so.Voltron.PlayTest.Protocol.Regulator
{
    /// <summary>
    /// Handles incoming <see cref="TSOPreAlphaSplitBufferPDU"/> PDUs from a remote connection
    /// </summary>
    [TSORegulator("TSOPlayTestSplitBufferPDUProtocol")]
    internal class SplitBufferPDUProtocol : TSOProtocol
    {
        internal class SplitBufferPDUThreadContext : IDisposable
        {
            private TSOSplitBufferPDUCollection _SplitBufferPDUs = new();
            /// <summary>
            /// When merging a <see cref="TSOPreAlphaSplitBufferPDU"/> this stores what packet we're currently unpacking
            /// </summary>
            private TSOVoltronPacketHeader? _VoltronPacketHeader;
            public uint _recvBytes = 0;
            public int _recvPDUs = 0;
            public bool IsUnpacking => _VoltronPacketHeader != null;

            internal void DoProtocolOnThread(TSOPDUFactoryServiceBase FactoryService, TSOVoltronPacket PDU, out TSOVoltronPacket? UnsplitPacket)
            {
                UnsplitPacket = null;

                _recvPDUs++;
                var splitBuffer = (TSOPlayTestSplitBufferPDU)PDU;
                if (!IsUnpacking)
                { // START UNPACKING                    
                    _VoltronPacketHeader = TSOVoltronPacket.ReadVoltronHeader(splitBuffer.DataBuffer);
                    _recvBytes = 0;
                }
                _recvBytes += splitBuffer.SplitBufferPayloadSize;
                _SplitBufferPDUs.Add(splitBuffer);

                if (_recvBytes >= _VoltronPacketHeader.PDUPayloadSize || splitBuffer.EOF)
                { // all packets received. dispose and reset
                    UnsplitPacket = FactoryService.CreatePacketObjectFromSplitBuffers(_SplitBufferPDUs);             
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

        [TSOProtocolHandler((uint)TSO_PlayTest_VoltronPacketTypes.SplitBufferPDU)]
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
                throw new Exception($"{nameof(TSOPlayTestSplitBufferPDU)} cannot create a new context for the thread: {ID}");
            context.DoProtocolOnThread(GetService<TSOPDUFactoryServiceBase>(), PDU, out TSOVoltronPacket? DesplitPDU);
            if (DesplitPDU != null)
            { // decompressed a PDU ... insert it into this voltron aries frame
                InsertOne(DesplitPDU);

                LogConsole($"Inserted the {DesplitPDU}\n\nFrom {context._recvPDUs} {nameof(TSOPlayTestSplitBufferPDU)}s ... ({context._recvBytes} bytes)");

                context.Dispose();
                _threads.TryRemove(ID, out _);
            }
        }
    }
}

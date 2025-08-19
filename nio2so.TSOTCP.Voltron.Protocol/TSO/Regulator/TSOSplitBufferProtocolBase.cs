using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO.Collections;
using nio2so.Voltron.Core.TSO.PDU;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.Core.TSO.Regulator
{
    /// <summary>
    /// A base class for SplitBuffer PDU behavior
    /// </summary>
    public abstract class TSOSplitBufferProtocolBase
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

            internal void DoProtocolOnThread(TSOPDUFactoryServiceBase FactoryService, TSOVoltronPacket PDU, out TSOVoltronPacket? UnsplitPacket)
            {
                UnsplitPacket = null;

                _recvPDUs++;
                var splitBuffer = (TSOSplitBufferPDUBase)PDU;
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
                throw new Exception($"{nameof(TSOSplitBufferPDUBase)} cannot create a new context for the thread: {ID}");
            context.DoProtocolOnThread(GetService<TSOPDUFactoryServiceBase>(), PDU, out TSOVoltronPacket? DesplitPDU);
            if (DesplitPDU != null)
            { // decompressed a PDU ... insert it into this voltron aries frame
                InsertOne(DesplitPDU);

                LogConsole($"Inserted the {DesplitPDU}\n\nFrom {context._recvPDUs} {nameof(TSOSplitBufferPDUBase)}s ... ({context._recvBytes} bytes)");

                context.Dispose();
                _threads.TryRemove(ID, out _);
            }
        }

        protected abstract void InsertOne(TSOVoltronPacket InsertionPacket);
        protected abstract void LogConsole(string message);
        protected abstract T GetService<T>() where T : ITSOService;
    }
}

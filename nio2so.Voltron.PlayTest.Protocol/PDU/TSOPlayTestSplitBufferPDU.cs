using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PlayTest.Protocol.PDU
{
    /// <summary>
    /// Wrapper for a data buffer larger than that of the current <see cref="TSOVoltronConst.TSOAriesClientBufferLength"/> on either party
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PlayTest_VoltronPacketTypes.SplitBufferPDU)]
    internal class TSOPlayTestSplitBufferPDU : TSOSplitBufferPDUBase
    {
        public const uint STANDARD_CHUNK_SIZE = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;
        public override ushort VoltronPacketType => (ushort)TSO_PlayTest_VoltronPacketTypes.SplitBufferPDU;

        /// <summary>
        /// Creates a new blank <see cref="TSOPreAlphaSplitBufferPDU"/>
        /// </summary>
        public TSOPlayTestSplitBufferPDU() : base() { MakeBodyFromProperties(); }
        /// <summary>
        /// Creates a new <see cref="TSOPreAlphaSplitBufferPDU"/> with the given data stream <paramref name="DataBuffer"/>        
        /// </summary>
        /// <param name="DataBuffer"></param>
        /// <param name="IsEOF">This <b>needs</b> to be accurate to whether there is forthcoming data bytes in future split buffer 
        /// transmissions in this frame -- as in, <b>has the whole data buffer been sent for this frame?</b></param>
        /// <exception cref="InternalBufferOverflowException"></exception>
        public TSOPlayTestSplitBufferPDU(byte[] DataBuffer, bool IsEOF) : base(DataBuffer, IsEOF) { }
    }
}

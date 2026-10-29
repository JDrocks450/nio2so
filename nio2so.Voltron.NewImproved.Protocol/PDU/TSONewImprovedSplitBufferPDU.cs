using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.PDU;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    /// <summary>
    /// Wrapper for a data buffer larger than that of the current <see cref="TSOVoltronConst.TSOAriesClientBufferLength"/> on either party
    /// </summary>
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.SplitBufferPDU)]
    internal class TSONewImprovedSplitBufferPDU : TSOSplitBufferPDUBase
    {
        public const uint STANDARD_CHUNK_SIZE = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.SplitBufferPDU;

        /// <summary>
        /// Creates a new blank <see cref="TSOPreAlphaSplitBufferPDU"/>
        /// </summary>
        public TSONewImprovedSplitBufferPDU() : base() { MakeBodyFromProperties(); }
        /// <summary>
        /// Creates a new <see cref="TSOPreAlphaSplitBufferPDU"/> with the given data stream <paramref name="DataBuffer"/>        
        /// </summary>
        /// <param name="DataBuffer"></param>
        /// <param name="IsEOF">This <b>needs</b> to be accurate to whether there is forthcoming data bytes in future split buffer 
        /// transmissions in this frame -- as in, <b>has the whole data buffer been sent for this frame?</b></param>
        /// <exception cref="InternalBufferOverflowException"></exception>
        public TSONewImprovedSplitBufferPDU(byte[] DataBuffer, bool IsEOF) : base(DataBuffer, IsEOF) { }
    }
}

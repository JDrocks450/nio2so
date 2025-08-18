using nio2so.Voltron.Core.TSO;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// Wrapper for a data buffer larger than that of the current <see cref="TSOVoltronConst.TSOAriesClientBufferLength"/> on either party
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.SPLIT_BUFFER_PDU)]
    public class TSOSplitBufferPDU : TSOVoltronPacket
    {
        public const uint STANDARD_CHUNK_SIZE = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SPLIT_BUFFER_PDU;

        /// <summary>
        /// Represents if we've reached the end of the split data stream yet with this <see cref="TSOSplitBufferPDU"/> <para/>
        /// This is <see langword="true"/> if there aren't more <see cref="TSOSplitBufferPDU"/> after this one
        /// </summary>
        public bool EOF { get; set; } = true;
        /// <summary>
        /// This <see cref="uint"/> indicates how many bytes are in this <see cref="DataBuffer"/>
        /// </summary>
        public uint SplitBufferPayloadSize { get; set; } = 0; // intentionally not using VoltronArrayLengthAttribute just in case of errors from refactors
        [TSOVoltronBodyArray] public byte[] DataBuffer { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Creates a new blank <see cref="TSOSplitBufferPDU"/>
        /// </summary>
        public TSOSplitBufferPDU() : base() { MakeBodyFromProperties(); }
        /// <summary>
        /// Creates a new <see cref="TSOSplitBufferPDU"/> with the given data stream <paramref name="DataBuffer"/>        
        /// </summary>
        /// <param name="DataBuffer"></param>
        /// <param name="IsEOF">This <b>needs</b> to be accurate to whether there is forthcoming data bytes in future split buffer 
        /// transmissions in this frame -- as in, <b>has the whole data buffer been sent for this frame?</b></param>
        /// <exception cref="InternalBufferOverflowException"></exception>
        public TSOSplitBufferPDU(byte[] DataBuffer, bool IsEOF) : this()
        {
            if (DataBuffer.Length > uint.MaxValue)
                throw new InternalBufferOverflowException("SPLIT_BUFFER_PDU cannot have a payload size above 0xFF!!!");

            this.DataBuffer = DataBuffer;
            EOF = IsEOF;
            SplitBufferPayloadSize = (uint)DataBuffer.Length;

            MakeBodyFromProperties();
        }

        /// <summary>
        /// Is <c>SplitBufferPayloadSize == DataBuffer.Length</c>?
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public override void EnsureNoErrors()
        {
            if (SplitBufferPayloadSize != DataBuffer.Length)
                throw new InvalidDataException("SplitBufferPDU reported size and actual size are not the same!!!");
            base.EnsureNoErrors();
        }
    }
}

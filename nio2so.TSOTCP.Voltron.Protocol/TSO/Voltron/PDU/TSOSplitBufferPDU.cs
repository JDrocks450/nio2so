using nio2so.TSOTCP.City.TSO.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SPLIT_BUFFER_PDU)]
    public class TSOSplitBufferPDU : TSOVoltronPacket
    {
        public const byte STANDARD_CHUNK_SIZE = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;

        /// <summary>
        /// True if there are more SPLIT_BUFFER_PDUs after this one
        /// </summary>
        [TSOVoltronIgnorable] public bool HasDataRemaining => DataRemaining == 0;
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SPLIT_BUFFER_PDU;

        /// <summary>
        /// This is 0x00 00 00 00 if there are more SPLIT_BUFFER_PDUs after this one
        /// </summary>
        public uint DataRemaining { get; set; }
        /// <summary>
        /// This byte indicates how many bytes are in this <see cref="TSOSplitBufferPDU"/>
        /// </summary>
        public byte SplitBufferPayloadSize { get; set; }
        [TSOVoltronBodyArray] public byte[] DataBuffer { get; set; }

        public TSOSplitBufferPDU() : base() { }

        public TSOSplitBufferPDU(byte[] DataBuffer, bool DataRemainingHereafter)
        {
            if (DataBuffer.Length > byte.MaxValue)
                throw new InternalBufferOverflowException("SPLIT_BUFFER_PDU cannot have a payload size above 0xFF!!!");

            this.DataBuffer = DataBuffer;
            DataRemaining = (uint)(DataRemainingHereafter ? 0x0 : 0x01000000);
            SplitBufferPayloadSize = (byte)DataBuffer.Length;

            MakeBodyFromProperties();
        }

        public override void EnsureNoErrors()
        {
            if (SplitBufferPayloadSize != DataBuffer.Length)
                throw new InvalidDataException("SplitBufferPDU reported size and actual size are not the same!!!");
        }
    }
}

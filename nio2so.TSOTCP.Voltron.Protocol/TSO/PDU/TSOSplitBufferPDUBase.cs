using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.Core.TSO.PDU
{
    public abstract class TSOSplitBufferPDUBase : TSOVoltronPacket
    {
        /// <summary>
        /// Represents if we've reached the end of the split data stream yet with this <see cref="TSOSplitBufferPDU"/> <para/>
        /// This is <see langword="true"/> if there aren't more <see cref="TSOSplitBufferPDU"/> after this one
        /// </summary>
        public bool EOF { get; set; }
        /// <summary>
        /// This <see cref="uint"/> indicates how many bytes are in this <see cref="DataBuffer"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(DataBuffer))] public uint SplitBufferPayloadSize { get; set; }
        /// <summary>
        /// The payload of this <see cref="TSOSplitBufferPDUBase"/>
        /// </summary>
        [TSOVoltronBodyArray]
        public byte[] DataBuffer { get; set; } = new byte[0];

        protected TSOSplitBufferPDUBase()
        {
            DataBuffer = Array.Empty<byte>();
            EOF = false;
            SplitBufferPayloadSize = 0;
            MakeBodyFromProperties();
        }

        protected TSOSplitBufferPDUBase(byte[] dataBuffer, bool isEOF) : this()
        {
            DataBuffer = dataBuffer;            
            EOF = isEOF;
            SplitBufferPayloadSize = (uint)dataBuffer.Length;
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

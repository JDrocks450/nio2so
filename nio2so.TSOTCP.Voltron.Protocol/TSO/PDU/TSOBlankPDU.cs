using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.Core.TSO.PDU
{
    /// <summary>
    /// A PDU with no structure, used when receiving a PDU format not yet implemented
    /// <para/>Simply houses the PDU data stream into the <see cref="PayloadArray"/> property
    /// <para/>Sometimes known as a <c>TSODebugPDU</c> in nio2so
    /// </summary>
    public class TSOBlankPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType { get; }

        [TSOVoltronBodyArray] public byte[] PayloadArray { get; set; } = Array.Empty<byte>();

        public TSOBlankPDU(uint PacketType)
        {
            VoltronPacketType = (ushort)PacketType;
            MakeBodyFromProperties();
        }
        public TSOBlankPDU(uint PacketType, byte[] BodyBytes) : this(PacketType)
        {
            PayloadArray = BodyBytes;
            MakeBodyFromProperties();
        }
    }
}

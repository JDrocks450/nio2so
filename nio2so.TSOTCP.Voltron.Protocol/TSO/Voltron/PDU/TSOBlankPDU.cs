using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// A PDU with no structure, used when receiving a PDU format not yet implemented
    /// <para/>Simply houses the PDU data stream into the <see cref="TSOBlankPDU.PayloadArray"/> property
    /// <para/>Sometimes known as a <c>TSODebugPDU</c> in nio2so
    /// </summary>
    public class TSOBlankPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType { get; }

        [TSOVoltronBodyArray] public byte[] PayloadArray { get; set; }

        public TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes PacketType)
        {
            VoltronPacketType = (ushort)PacketType;
            MakeBodyFromProperties();
        }
        public TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes PacketType, byte[] BodyBytes) : this(PacketType)
        {
            PayloadArray = BodyBytes;
            MakeBodyFromProperties();
        }
    }
}

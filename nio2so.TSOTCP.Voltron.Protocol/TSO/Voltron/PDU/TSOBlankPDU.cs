using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
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

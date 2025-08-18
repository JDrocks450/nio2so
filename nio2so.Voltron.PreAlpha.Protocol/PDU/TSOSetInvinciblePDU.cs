using nio2so.Voltron.Core.TSO;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU)]
    public class TSOSetInvinciblePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU;
        public TSOSetInvinciblePDU(uint value = 0x0)
        {
            Value = value;
            MakeBodyFromProperties();
        }

        public TSOSetInvinciblePDU()
        {
        }

        public uint Value { get; set; }
    }
}

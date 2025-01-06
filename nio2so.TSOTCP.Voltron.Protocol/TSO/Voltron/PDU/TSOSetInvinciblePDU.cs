namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU)]
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

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU)]
    public class TSOSetInvisiblePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU;
        public TSOSetInvisiblePDU(uint value = 0x0)
        {
            Value = value;
            MakeBodyFromProperties();
        }

        public TSOSetInvisiblePDU()
        {
        }

        public uint Value { get; set; }
    }
}

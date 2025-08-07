namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU)]
    public class TSOSetIgnoreListPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU;
        public TSOSetIgnoreListPDU(ushort value = 0x0)
        {
            Value = value;
            MakeBodyFromProperties();
        }

        public TSOSetIgnoreListPDU()
        {
        }

        public ushort Value { get; set; }
    }
}

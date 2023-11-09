namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU)]
    internal class TSOSetIgnoreListPDU : TSOVoltronPacket
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
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_RESPONSE_PDU)]
    internal class TSOSetIgnoreListResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_RESPONSE_PDU;

        public TSOSetIgnoreListResponsePDU()
        {
            MakeBodyFromProperties();
        }

        public ushort NumberOfPlayers { get; set; } = 0x0;
        //TODO: http://wiki.niotso.org/Maxis_Protocol#SetIgnoreListResponsePDU
    }
}

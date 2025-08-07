namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU)]
    public class TSOSetAcceptFlashesPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU;
        public TSOSetAcceptFlashesPDU(uint value = 0x0)
        {
            Value = value;
            MakeBodyFromProperties();
        }

        public TSOSetAcceptFlashesPDU()
        {
        }

        public uint Value { get; set; }
    }
}

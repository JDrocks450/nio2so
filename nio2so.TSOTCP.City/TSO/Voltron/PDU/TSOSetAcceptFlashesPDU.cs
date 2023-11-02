namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU)]
    internal class TSOSetAcceptFlashesPDU : TSOVoltronPacket
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
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_RESPONSE_PDU)]
    internal class TSOSetAcceptFlashesResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_RESPONSE_PDU;

        public TSOSetAcceptFlashesResponsePDU(bool AcceptsFlashes, uint statusCode = 200, string reasonText = "OK.")
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            AcceptingFlashes = (byte)(AcceptsFlashes ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public byte AcceptingFlashes { get; set; } = 0x0;
    }
}

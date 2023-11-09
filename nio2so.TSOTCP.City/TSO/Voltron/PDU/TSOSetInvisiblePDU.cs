namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU)]
    internal class TSOSetInvisiblePDU : TSOVoltronPacket
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
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_RESPONSE_PDU)]
    internal class TSOSetInvisibleResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_RESPONSE_PDU;

        public TSOSetInvisibleResponsePDU(bool IsInvisible, uint statusCode = 200, string reasonText = "OK.")
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            CurrentlyInvisible = (byte)(IsInvisible ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public byte CurrentlyInvisible { get; set; } = 0x0;
    }
}

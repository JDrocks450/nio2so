namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU)]
    internal class TSOSetInvinciblePDU : TSOVoltronPacket
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
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_RESPONSE_PDU)]
    internal class TSOSetInvincibleResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_RESPONSE_PDU;

        public TSOSetInvincibleResponsePDU(bool IsInvincible, uint statusCode = 200, string reasonText = "OK.")
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            CurrentlyInvincible = (byte)(IsInvincible ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public byte CurrentlyInvincible { get; set; } = 0x0;
    }
}

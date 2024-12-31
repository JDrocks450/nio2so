using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU)]
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
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_RESPONSE_PDU)]
    internal class TSOSetInvincibleResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_RESPONSE_PDU;

        public TSOSetInvincibleResponsePDU(bool IsInvincible,
            uint statusCode = TSOVoltronConst.ResponsePDU_DefaultStatusCode,
            string reasonText = TSOVoltronConst.ResponsePDU_DefaultReasonText)
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

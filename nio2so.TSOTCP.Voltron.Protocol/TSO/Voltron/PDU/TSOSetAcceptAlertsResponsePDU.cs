using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_RESPONSE_PDU)]
    public class TSOSetAcceptAlertsResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptAlertsResponsePDU(bool AcceptsAlerts,
            uint statusCode = TSOVoltronConst.ResponsePDU_DefaultStatusCode,
            string reasonText = TSOVoltronConst.ResponsePDU_DefaultReasonText)
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            AcceptingAlerts = (byte)(AcceptsAlerts ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public byte AcceptingAlerts { get; set; } = 0x0;
    }
}

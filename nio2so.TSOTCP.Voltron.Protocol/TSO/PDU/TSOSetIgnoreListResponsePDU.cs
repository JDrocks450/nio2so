using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_RESPONSE_PDU)]
    public class TSOSetIgnoreListResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_RESPONSE_PDU;

        public TSOSetIgnoreListResponsePDU(bool IsInvincible,
            uint statusCode = TSOVoltronConst.ResponsePDU_DefaultStatusCode,
            string reasonText = TSOVoltronConst.ResponsePDU_DefaultReasonText)
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            MaxNumberOfIgnored = (byte)(IsInvincible ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public uint MaxNumberOfIgnored { get; set; } = 0x0;
        //TODO: http://wiki.niotso.org/Maxis_Protocol#SetIgnoreListResponsePDU
    }
}

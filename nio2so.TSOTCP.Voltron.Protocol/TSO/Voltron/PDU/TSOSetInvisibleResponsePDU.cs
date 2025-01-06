using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_RESPONSE_PDU)]
    public class TSOSetInvisibleResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_RESPONSE_PDU;

        public TSOSetInvisibleResponsePDU(bool IsInvisible, uint statusCode = TSOVoltronConst.ResponsePDU_DefaultStatusCode,
            string reasonText = TSOVoltronConst.ResponsePDU_DefaultReasonText)
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

using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{

    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_RESPONSE_PDU)]
    public class TSOSetAcceptAlertsResponsePDU : TSOVoltronBasicResponsePacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptAlertsResponsePDU() : this(true) { }
        public TSOSetAcceptAlertsResponsePDU(bool AcceptingAlerts, TSOStatusReasonStruct? StatusReason = default) : base(AcceptingAlerts, StatusReason) { }
    }
}

using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{

    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_RESPONSE_PDU)]
    public class TSOSetAcceptAlertsResponsePDU : TSOVoltronBasicResponsePacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptAlertsResponsePDU() : this(true) { }
        public TSOSetAcceptAlertsResponsePDU(bool AcceptingAlerts, TSOStatusReasonStruct? StatusReason = default) : base(AcceptingAlerts, StatusReason) { }
    }
}

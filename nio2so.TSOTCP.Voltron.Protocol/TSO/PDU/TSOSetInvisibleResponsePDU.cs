using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_RESPONSE_PDU)]
    public class TSOSetInvisibleResponsePDU : TSOVoltronBasicResponsePacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetInvisibleResponsePDU(bool IsInvisible, TSOStatusReasonStruct? StatusReason = default) : base(IsInvisible, StatusReason) { }
        public TSOSetInvisibleResponsePDU() : this(true) { }
    }
}

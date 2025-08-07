using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_RESPONSE_PDU)]
    public class TSOSetInvincibleResponsePDU : TSOVoltronBasicResponsePacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetInvincibleResponsePDU(bool IsInvincible, TSOStatusReasonStruct? StatusReason = default) : base(IsInvincible, StatusReason) { }
        public TSOSetInvincibleResponsePDU() : this(true) { }
    }
}

using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_RESPONSE_PDU)]
    public class TSOSetAcceptFlashesResponsePDU : TSOVoltronBasicResponsePacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptFlashesResponsePDU() : this(true) { }
        public TSOSetAcceptFlashesResponsePDU(bool AcceptingFlashes, TSOStatusReasonStruct? StatusReason = default) : base(AcceptingFlashes, StatusReason) { }
    }
}

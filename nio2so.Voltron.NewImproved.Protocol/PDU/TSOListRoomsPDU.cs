using nio2so.Voltron.Core.TSO;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.List20RoomsPDU)]
    public class TSOList20RoomsPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.List20RoomsPDU;
    }
}

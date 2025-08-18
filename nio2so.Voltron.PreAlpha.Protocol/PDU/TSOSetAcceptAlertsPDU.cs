using nio2so.Voltron.Core.TSO;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU)]
    public class TSOSetAcceptAlertsPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptAlertsPDU()
        {
        }
        public TSOSetAcceptAlertsPDU(uint value = 0x0)
        {
            AcceptingAlerts = value;
            MakeBodyFromProperties();
        }

        public uint AcceptingAlerts { get; set; }
    }
}

using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU)]
    public class TSOFindPlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU;

        public TSOAriesIDStruct RequestedPlayer { get; set; }

        public TSOFindPlayerPDU() : base()
        {
            MakeBodyFromProperties();
        }
    }
}

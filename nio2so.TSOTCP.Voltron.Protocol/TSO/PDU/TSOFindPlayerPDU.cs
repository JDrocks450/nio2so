using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU)]
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

using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// 
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.EXIT_ROOM_PDU)]
    public class TSOExitRoomPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.EXIT_ROOM_PDU;

        public TSOAriesIDStruct PlayerInfo { get; set; }

        public TSOExitRoomPDU()
        {
            MakeBodyFromProperties();
        }

        public TSOExitRoomPDU(TSOAriesIDStruct playerInfo)
        {
            PlayerInfo = playerInfo;
            MakeBodyFromProperties();
        }
    }
}

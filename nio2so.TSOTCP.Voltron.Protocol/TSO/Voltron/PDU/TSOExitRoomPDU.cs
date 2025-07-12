using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// 
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.EXIT_ROOM_PDU)]
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

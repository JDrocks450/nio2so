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
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.JOIN_ROOM_PDU)] 
    public class TSOJoinRoomPDU : TSOVoltronPacket
    {
        public TSOJoinRoomPDU()
        {
        }

        public TSOJoinRoomPDU(TSORoomIDStruct roomID, string password = "")
        {
            RoomID = roomID;
            Password = password;
        }

        public TSORoomIDStruct RoomID { get; set; } = new(0,"");
        public string Password { get; set; } = "";
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.JOIN_ROOM_PDU;
    }
}

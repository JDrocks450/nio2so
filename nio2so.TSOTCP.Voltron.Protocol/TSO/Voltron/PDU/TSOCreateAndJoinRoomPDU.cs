using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Verified
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.CREATE_AND_JOIN_ROOM_PDU)]
    public class TSOCreateAndJoinRoomPDU : TSOVoltronPacket
    {
        public TSOCreateAndJoinRoomPDU(TSOCreateRoomInfo createRoomInfo)
        {
            CreateRoomInfo = createRoomInfo;
            MakeBodyFromProperties();
        }

        public TSOCreateRoomInfo CreateRoomInfo { get; set; }
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.CREATE_AND_JOIN_ROOM_PDU;
    }
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.CREATE_ROOM_RESPONSE_PDU)]
    public class TSOCreateRoomResponsePDU : TSOVoltronPacket
    {
        public TSOCreateRoomResponsePDU(TSORoomIDStruct RoomID)
        {
            this.RoomID = RoomID;
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; } = 0;
        public string ReasonText { get; set; } = "";
        public TSORoomIDStruct RoomID { get; set; }
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.CREATE_ROOM_RESPONSE_PDU;
    }
}

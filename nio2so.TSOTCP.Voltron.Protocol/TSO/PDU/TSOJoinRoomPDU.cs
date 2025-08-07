using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
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

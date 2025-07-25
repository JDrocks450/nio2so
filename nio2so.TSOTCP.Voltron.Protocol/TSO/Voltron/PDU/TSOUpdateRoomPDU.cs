using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent to a remote party to update which room it is currently in
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU)]
    public class TSOUpdateRoomPDU : TSOVoltronPacket
    {               
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU;

        public TSOUpdateRoomPDU() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOUpdateRoomPDU"/> with the given information                   
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="RoomInfo"></param>
        /// <param name="DataStartByte"></param>
        public TSOUpdateRoomPDU(uint RoomID, TSORoomInfoStruct RoomInfo, byte DataStartByte = 0x00)
        {
            this.RoomID = RoomID;
            this.DataStartByte = DataStartByte;
            NewRoomInfo = RoomInfo;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// The RoomID of the Room
        /// </summary>
        public uint RoomID { get; set; }
        public byte DataStartByte { get; set; } = 0x01;
        /// <summary>
        /// The new <see cref="TSORoomInfoStruct"/> the client should update itself to a be a member of
        /// </summary>
        public TSORoomInfoStruct NewRoomInfo { get; set; }
    }
}

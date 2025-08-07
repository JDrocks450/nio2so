using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    /// <summary>
    /// Verified
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.CREATE_AND_JOIN_ROOM_PDU)]
    public class TSOCreateAndJoinRoomPDU : TSOVoltronPacket
    {
        public TSOCreateAndJoinRoomPDU() : base()
        {
        }

        public TSOCreateAndJoinRoomPDU(TSOCreateRoomInfo createRoomInfo) : this()
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
        public TSOCreateRoomResponsePDU() : base()
        {
        }

        public TSOCreateRoomResponsePDU(TSORoomIDStruct RoomID) : this()
        {
            this.RoomID = RoomID;
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; } = 0;
        public string ReasonText { get; set; } = "";
        public TSORoomIDStruct RoomID { get; set; } = TSORoomIDStruct.Blank;
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.CREATE_ROOM_RESPONSE_PDU;
    }
}

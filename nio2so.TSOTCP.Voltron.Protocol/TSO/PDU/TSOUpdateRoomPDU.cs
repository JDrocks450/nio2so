using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
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
        /// <param name="ReasonCode"></param>
        /// <param name="RoomInfo"></param>
        /// <param name="DataStartByte"></param>
        public TSOUpdateRoomPDU(uint ReasonCode, TSORoomInfoStruct RoomInfo, bool OccupantListFollows = true)
        {
            this.ReasonCode = ReasonCode;
            NewRoomInfo = RoomInfo;
            this.OccupantListFollows = OccupantListFollows;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// The RoomID of the Room
        /// </summary>
        public uint ReasonCode { get; set; }
        public bool OccupantListFollows { get; set; } = false;
        /// <summary>
        /// The new <see cref="TSORoomInfoStruct"/> the client should update itself to a be a member of
        /// </summary>
        public TSORoomInfoStruct NewRoomInfo { get; set; }
#if false
        /// <summary>
        /// <inheritdoc cref="TSOVoltronArrayLength"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(OccupantList))]
        public uint OccupantListCount { get; set; }
        /// <summary>
        /// A list of user <see cref="TSOAriesIDStruct"/> in this room
        /// </summary>
        public TSOAriesIDStruct[] OccupantList { get; set; } = new TSOAriesIDStruct[0];
#endif
    }
}

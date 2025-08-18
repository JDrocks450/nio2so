using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU)]
    public class TSOFindPlayerResponsePDU : TSOVoltronPacket
    {        
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU;

        public TSOStatusReasonStruct StatusReason { get; set; } = TSOStatusReasonStruct.Online;

        public TSORoomInfoStruct RoomInfo { get; set; } = TSORoomInfoStruct.NoRoom;

        public TSOPlayerInfoStruct PlayerInfo { get; set; } = new();

        /// <summary>
        /// Creates a new <see cref="TSOFindPlayerResponsePDU"/>
        /// </summary>
        public TSOFindPlayerResponsePDU() : base()
        {
            MakeBodyFromProperties();
        }
        /// <summary>
        /// <inheritdoc cref="TSOFindPlayerResponsePDU()"/> with the <paramref name="PlayerID"/> and <paramref name="Status"/> provided
        /// </summary>
        /// <param name="PlayerID"></param>
        /// <param name="Status">If default, will respond <see cref="TSOStatusReasonStruct.Online"/></param>
        public TSOFindPlayerResponsePDU(TSOPlayerInfoStruct PlayerInfo, TSOStatusReasonStruct? Status = default, TSORoomInfoStruct CurrentRoom = default) : this()
        {
            if (Status == default) Status = TSOStatusReasonStruct.Online;
            if (CurrentRoom == default) CurrentRoom = TSORoomInfoStruct.NoRoom;
            RoomInfo = CurrentRoom;
            StatusReason = Status;
            this.PlayerInfo = PlayerInfo;
            MakeBodyFromProperties();
        }
    }
}

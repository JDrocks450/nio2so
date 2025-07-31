using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU)]
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
        public TSOFindPlayerResponsePDU(TSOPlayerInfoStruct PlayerInfo, TSOStatusReasonStruct? Status = default) : this()
        {
            if (Status == default) Status = TSOStatusReasonStruct.Online;
            StatusReason = Status;
            this.PlayerInfo = PlayerInfo;
            MakeBodyFromProperties();
        }
    }
}

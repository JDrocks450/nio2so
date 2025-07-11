using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU)]
    public class TSOFindPlayerResponsePDU : TSOVoltronPacket
    {
        public enum PlayerStatuses
        {
            Online = 0x0,
            Offline = 0x01,
            LetterOnly = 0x21,
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU;

        public uint ReasonCode { get; set; }
        
        public string ReasonText { get; set; } = "OK";

        public TSORoomInfo RoomInfo { get; set; } = TSORoomInfo.NoRoom;

        public TSOPlayerInfoStruct PlayerInfo { get; set; }

        public TSOFindPlayerResponsePDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOFindPlayerResponsePDU(TSOAriesIDStruct PlayerID, uint StatusCode) : this()
        {
            ReasonCode = StatusCode;            
            PlayerInfo = new(PlayerID,1,true);
            MakeBodyFromProperties();
        }
    }
}

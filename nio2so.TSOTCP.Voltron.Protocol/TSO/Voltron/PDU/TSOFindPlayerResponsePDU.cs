using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU)]
    public class TSOFindPlayerResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU;

        public TSOAriesIDStruct PlayerID { get; set; }

        public uint StatusCode { get; set; } = 0x21;
        public uint StatusCode1 { get; set; } = 0;
        public uint StatusCode2 { get; set; } = 0;

        public TSOFindPlayerResponsePDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOFindPlayerResponsePDU(TSOAriesIDStruct PlayerID, uint StatusCode) : this()
        {
            this.PlayerID = PlayerID;
            this.StatusCode = StatusCode;
            MakeBodyFromProperties();
        }
    }
}

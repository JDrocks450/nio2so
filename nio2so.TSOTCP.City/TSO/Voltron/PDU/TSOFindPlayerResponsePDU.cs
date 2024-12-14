using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU)]
    internal class TSOFindPlayerResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU;

        public uint StatusCode { get; set; } = 21;
        [TSOVoltronString] public string ReasonString { get; set; } = "Test error message";

        public TSOFindPlayerResponsePDU() : base() {
            MakeBodyFromProperties();
        }    

        public TSOFindPlayerResponsePDU(uint StatusCode) : this()
        {
            this.StatusCode = StatusCode;
            MakeBodyFromProperties();
        }
    }
}

using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU)]
    public class TSOFindPlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU;

        [TSOVoltronString()] public string AriesID { get; set; } = "";
        [TSOVoltronString()] public string MasterID { get; set; } = "";

        public TSOFindPlayerPDU() : base() { }

        public TSOFindPlayerPDU(string ariesID, string masterID) : base()
        {
            AriesID = ariesID;
            MasterID = masterID;
            MakeBodyFromProperties();
        }
    }
}

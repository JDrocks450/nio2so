using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
    public class TSOBCVersionListPDU : TSOVoltronPacket
    {
        public TSOBCVersionListPDU() : base() { }
        public TSOBCVersionListPDU(string versionString, string str1, uint arg1)
        {
            VersionString = versionString;
            Str1 = str1;
            Arg1 = arg1;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU;
        [TSOVoltronString] public string VersionString { get; set; }
        [TSOVoltronString]
        public string Str1 { get; set; }
        public uint Arg1 { get; set; }
    }
}

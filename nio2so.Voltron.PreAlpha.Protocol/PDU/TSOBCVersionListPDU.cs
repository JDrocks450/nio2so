using nio2so.Voltron.Core.TSO;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
    public class TSOBCVersionListPDU : TSOVoltronPacket
    {
        public TSOBCVersionListPDU() : base() { }
        public TSOBCVersionListPDU(string AvatarID, string VersionString, uint arg1)
        {
            this.AvatarID = AvatarID;
            this.VersionString = VersionString;
            Version = arg1;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU;
        [TSOVoltronString] public string AvatarID { get; set; }
        [TSOVoltronString]
        public string VersionString { get; set; }
        public uint Version { get; set; }
    }
}

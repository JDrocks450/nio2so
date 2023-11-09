using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU)]
    internal class TSOBCVersionListPDU : TSOVoltronPacket
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
        public string VersionString { get; set; }
        [TSOVoltronString]
        public string Str1 { get; set; }
        public uint Arg1 { get; set; }
    }
}

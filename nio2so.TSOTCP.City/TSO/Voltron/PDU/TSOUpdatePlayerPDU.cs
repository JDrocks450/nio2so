using MiscUtil.Conversion;
using nio2so.TSOTCP.City.TSO.Aries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{

    internal class TSOUpdatePlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU;
        public uint Arg1 { get; set;  }
        public string AriesID { get; set; }
        public string MasterID { get; set; }
        public byte Arg2 { get; set; } = 0x41;
        public byte Arg3 { get; set; } = 0x01;

        public TSOUpdatePlayerPDU(string ariesID, string masterID, uint arg1 = 0x1C)
        {
            Arg1 = arg1;
            AriesID = ariesID;
            MasterID = masterID;
            MakeBodyFromProperties();
        }
    }

#if false
    internal class TSOHostOnlinePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => 0x1D;
        public ushort NumberOfWords => (ushort)HostReservedWords.Length;
        public string[] HostReservedWords { get; }
        public ushort HostVersion { get; }
        public ushort SendLimit { get; }

        protected TSOHostOnlinePDU(TSOTCPPacket Packet) : base(Packet)
        {

        }

        public TSOHostOnlinePDU(ushort HostVersion, ushort SendLimit = TSOCityServer.TSO_Aries_SendRecvLimit, params string[] HostReservedWords)
        {
            this.HostVersion = HostVersion;
            this.SendLimit = SendLimit;
            this.HostReservedWords = HostReservedWords;
            ReflectProperties2Buffer();
        }

        public override T ParseFromAriesPacket<T>(TSOTCPPacket AriesPacket)
        {
            throw new NotImplementedException();
        }
    }
#endif
}

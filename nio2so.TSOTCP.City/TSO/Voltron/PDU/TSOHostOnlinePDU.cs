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
    internal class TSOHostOnlinePDU : TSOVoltronPacket
    {
        //CONST/TUNING
        const ushort PACKET_SIZE_LIMIT = TSO.TSOCityServer.DefaultSendAmt;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU;
        public uint HostVersion { get; } 
        public ushort SizeLimit { get; }               
        public uint Arg3 { get; }

        public TSOHostOnlinePDU(uint hostVersion = 0x0C, ushort packetSize = PACKET_SIZE_LIMIT, uint arg3 = 0x0)
        {
            HostVersion = hostVersion;
            SizeLimit = packetSize;
            Arg3 = arg3;
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

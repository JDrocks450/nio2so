//#define NIOTSO_FORMAT

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
    /// <summary>
    /// The <see cref="TSOHostOnlinePDU"/> sends to the Client that this server is a Voltron server.
    /// <para>Encompasses a HostVersion and PacketSize</para>
    /// </summary>
    internal class TSOHostOnlinePDU : TSOVoltronPacket
    {
        //CONST/TUNING
        const ushort PACKET_SIZE_LIMIT = TSO.TSOCityServer.DefaultSendAmt;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU;
#if NIOTSO_FORMAT
        public ushort HostVersion { get; }
        public uint SizeLimit { get; }
        public uint Arg3 { get; }
        public TSOHostOnlinePDU(ushort hostVersion = 0x0, uint packetSize = 0x0C0000, uint arg3 = 0x7FFF7FFF)
#else
        public uint HostVersion { get; } 
        public ushort SizeLimit { get; }               
        public uint Arg3 { get; }
        public TSOHostOnlinePDU(uint hostVersion = 0x0C, ushort packetSize = PACKET_SIZE_LIMIT, uint arg3 = 0x7FFF7FFF)
#endif        
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

using MiscUtil.Conversion;
using nio2so.TSOTCP.City.TSO.Aries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// The <see cref="TSOHostOnlinePDU"/> sends to the Client that this server is a Voltron server.
    /// <para>Encompasses a HostVersion and PacketSize</para>
    /// </summary>
    public class TSOHostOnlinePDU : TSOVoltronPacket
    {
        //CONST/TUNING
        const ushort PACKET_SIZE_LIMIT = 0x0;// TSO.TSOCityServer.DefaultSendAmt;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU;

        public ushort SizeLimit { get; }
        public uint Arg3 { get; }
        public TSOHostOnlinePDU(ushort packetSize = PACKET_SIZE_LIMIT, uint arg3 = 0x7FFF7FFF)
        {
            SizeLimit = packetSize;
            Arg3 = arg3;
            MakeBodyFromProperties();
        }
    }
}

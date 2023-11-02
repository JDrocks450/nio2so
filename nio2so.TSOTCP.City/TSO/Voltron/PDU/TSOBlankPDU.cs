using nio2so.TSOTCP.City.TSO.Aries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    internal class TSOBlankPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType { get; }

        public TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes PacketType) {
            VoltronPacketType = (ushort)PacketType;
            MakeBodyFromProperties();
        }
    }
}

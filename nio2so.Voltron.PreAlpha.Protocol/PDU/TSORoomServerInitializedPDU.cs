using nio2so.Voltron.Core.TSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// Blank structure, untested, never seen this in practice. Currently used by nio2so to initialize the HSB Server.
    /// </summary>
    [TSOVoltronPDU((ushort)TSO_PreAlpha_VoltronPacketTypes.ROOMSERVER_INITIALIZED_PDU)] internal class TSORoomServerInitializedPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.ROOMSERVER_INITIALIZED_PDU;

        public TSORoomServerInitializedPDU() : base() { MakeBodyFromProperties(); }
    }
}

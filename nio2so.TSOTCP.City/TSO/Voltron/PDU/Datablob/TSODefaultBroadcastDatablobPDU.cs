using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob
{
    /// <summary>
    /// A <see cref="TSOBroadcastDatablobPacket"/> that dumps the packet data to the MessageContent property. 
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU)]
    internal class TSODefaultBroadcastDatablobPDU : TSOBroadcastDatablobPacket
    {
        /// <summary>
        /// The packet data, dumped to this property
        /// </summary>
        [TSOVoltronBroadcastDatablobPDUField] [TSOVoltronBodyArray] public byte[] MessageContent { get; set; } = new byte[0];
    }
}

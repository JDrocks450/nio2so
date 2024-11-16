using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    /// <summary>
    /// A PDU sent from the Client when asking for a House to load.
    /// <para>Found to be called after SetHouseBlobByID was sent from the server</para>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
    internal class TSOLoadHousePDU : TSOVoltronPacket
    {
        /// <summary>
        /// Can be absent from packet body in some circumstances
        /// </summary>
        public uint HouseID { get; set; }

        public TSOLoadHousePDU()
        {

        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU;
    }
}

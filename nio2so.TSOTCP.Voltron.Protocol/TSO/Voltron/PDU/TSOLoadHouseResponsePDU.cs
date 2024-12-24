using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    /// <summary>
    /// <c>WARNING:</c>
    /// <para>The TSOClient seems to use this packet wrong. Sucks. Therefore, in current spec, this packet type is never actually 
    /// sent to the Client, and <see cref="TSO_PreAlpha_VoltronPacketTypes.HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU"/> will be sent 
    /// instead.</para>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
    public class TSOLoadHouseResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU;
        /// <summary>
        /// One UInt32 with an ID to load.
        /// </summary>
        public uint HouseID { get; set; }
        public TSOLoadHouseResponsePDU() : base() { MakeBodyFromProperties(); }

        public TSOLoadHouseResponsePDU(uint houseID) : base()
        {
            HouseID = houseID;
            MakeBodyFromProperties();
        }
    }
}

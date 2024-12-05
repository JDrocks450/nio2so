using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent when the remote connection clicks "Join House" in the interface
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
    internal class TSOLotEntryRequestPDU : TSOVoltronPacket
    {
        public ushort Lot_X { get; set; }
        public ushort Lot_Y { get; set; }
        public uint HouseID { get; set; }

        public TSOLotEntryRequestPDU() : base()
        {

        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU;
    }
}

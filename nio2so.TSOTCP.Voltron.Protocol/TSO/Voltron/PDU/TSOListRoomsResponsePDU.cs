using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_RESPONSE_PDU)]
    internal class TSOListRoomsResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_RESPONSE_PDU;

        public uint NumRooms { get; }

        public TSOListRoomsResponsePDU(params uint[] RoomIDs) : base()
        {
            NumRooms = (uint)RoomIDs.Length;
            MakeBodyFromProperties();
        }
    }
}

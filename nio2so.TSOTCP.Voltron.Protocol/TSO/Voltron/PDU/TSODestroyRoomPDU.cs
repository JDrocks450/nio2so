using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU)]
    internal class TSODestroyRoomPDU : TSOVoltronPacket
    {
        /// <summary>
        /// The name of the Room
        /// </summary>
        [TSOVoltronString] public string RoomName { get; set; }
        /// <summary>
        /// Perhaps the session host or the person making the request to close the room?
        /// </summary>
        public TSOAriesIDStruct AriesID { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU;

        public TSODestroyRoomPDU() : base()
        {
            MakeBodyFromProperties();
        }
    }
}

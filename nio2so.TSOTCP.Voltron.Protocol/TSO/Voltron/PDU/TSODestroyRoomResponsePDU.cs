using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent to a Client requesting to close the current room it is in using <see cref="TSODestroyRoomPDU"/>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_RESPONSE_PDU)]
    public sealed class TSODestroyRoomResponsePDU : TSOVoltronPacket
    {
        /// <summary>
        /// Creates a new <see cref="TSODestroyRoomResponsePDU"/>
        /// </summary>
        public TSODestroyRoomResponsePDU() : base()
        {
            MakeBodyFromProperties();
        }
        /// <summary>
        /// <inheritdoc cref="TSODestroyRoomResponsePDU()"/> with the provided parameters
        /// </summary>
        /// <param name="RoomName"></param>
        /// <param name="HostName"></param>
        public TSODestroyRoomResponsePDU(TSOStatusReasonStruct StatusReason, TSORoomIDStruct RoomID)
        {
            this.StatusReason = StatusReason;
            this.RoomID = RoomID;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_RESPONSE_PDU;
        
        public TSOStatusReasonStruct StatusReason { get; set; }
        public TSORoomIDStruct RoomID { get; set; }
    }    
}

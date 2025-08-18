using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// Sent to a Client requesting to close the current room it is in using <see cref="TSODestroyRoomPDU"/>
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_RESPONSE_PDU)]
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

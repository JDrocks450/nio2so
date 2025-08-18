using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// Sent from a Client when it deems it necessary to close the current room it is in
    /// <para/>Should respond with the <see cref="TSODestroyRoomResponsePDU"/>
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU)]
    public sealed class TSODestroyRoomPDU : TSOVoltronPacket
    {
        /// <summary>
        /// The <see cref="TSORoomIDStruct"/>
        /// </summary>
        public TSORoomIDStruct RoomID { get; set; }
        
        /// <summary>
        /// Unknown
        /// </summary>
        [TSOVoltronString] public string ReasonText { get; set; }        

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU;

        public TSODestroyRoomPDU() : base()
        {
            MakeBodyFromProperties();
        }
    }
}

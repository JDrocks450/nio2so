using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// A PDU that is sent from a Client of a room host when they're leaving
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.DETACH_FROM_ROOM_PDU)]
    public class TSODetachFromRoomPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DETACH_FROM_ROOM_PDU;
        public TSORoomIDStruct RoomID { get; set; } = new();

        public TSODetachFromRoomPDU() : base() { MakeBodyFromProperties(); }

        public TSODetachFromRoomPDU(TSORoomIDStruct roomID) : this()
        {
            RoomID = roomID;
            MakeBodyFromProperties();
        }
    }
}

using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    /// <summary>
    /// A PDU that is sent from a Client of a room host when they're leaving
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.DETACH_FROM_ROOM_PDU)]
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

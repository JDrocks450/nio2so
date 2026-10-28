using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    public abstract class TSOListRoomsResponsePDUBase : TSOVoltronPacket
    {
        /// <summary>
        /// Size to the end of the packet from AFTER this property using the <see cref="TSOVoltronDistanceToEnd"/> attribute
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ListRoomBodyLength { get; set; }
        /// <summary>
        /// Unsure what this is supposed to be, I think it is the city name
        /// </summary>
        [TSOVoltronString] public string ServiceID { get; set; } = "";
        /// <summary>
        /// The amount of room objects added to this list
        /// </summary>
        [TSOVoltronValue(TSOVoltronValueTypes.BigEndian)][TSOVoltronArrayLength(nameof(Rooms))] public uint RoomCount { get; set; }

        /// <summary>
        /// Rooms that should be sent to the client
        /// </summary>
        public TSORoomInfoStruct[] Rooms { get; set; } = new TSORoomInfoStruct[0];

        /// <summary>
        /// Creates a new <see cref="TSOListRoomsResponsePDU"/> that sends a list of <see cref="TSORoomInfoStruct"/> objects to the client
        /// </summary>
        /// <param name="Rooms"></param>
        public TSOListRoomsResponsePDUBase(params TSORoomInfoStruct[] Rooms) : this()
        {
            if (Rooms.Any())
                this.Rooms = Rooms;
            else this.Rooms = [TSORoomInfoStruct.NoRoom];

            MakeBodyFromProperties();
        }

        public TSOListRoomsResponsePDUBase() : base() { MakeBodyFromProperties(); }
    }

    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.List20RoomsResponsePDU)]
    public class TSOList20RoomsResponsePDU : TSOListRoomsResponsePDUBase
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.List20RoomsResponsePDU;

        /// <summary>
        /// <inheritdoc cref="TSOListRoomsResponsePDUBase.TSOListRoomsResponsePDUBase(TSORoomInfoStruct[])"/>
        /// </summary>
        /// <param name="Rooms"></param>
        public TSOList20RoomsResponsePDU(params TSORoomInfoStruct[] Rooms) : base(Rooms) => MakeBodyFromProperties();

        public TSOList20RoomsResponsePDU() : base() => MakeBodyFromProperties();
    }

    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.ListRoomsResponsePDU)]
    public class TSOListRoomsResponsePDU : TSOListRoomsResponsePDUBase
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.ListRoomsResponsePDU;

        /// <summary>
        /// <inheritdoc cref="TSOListRoomsResponsePDUBase.TSOListRoomsResponsePDUBase(TSORoomInfoStruct[])"/>
        /// </summary>
        /// <param name="Rooms"></param>
        public TSOListRoomsResponsePDU(params TSORoomInfoStruct[] Rooms) : base(Rooms) => MakeBodyFromProperties();

        public TSOListRoomsResponsePDU() : base() => MakeBodyFromProperties();
    }
}

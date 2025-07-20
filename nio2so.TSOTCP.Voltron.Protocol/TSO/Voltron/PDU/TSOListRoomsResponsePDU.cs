using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_RESPONSE_PDU)]
    public class TSOListRoomsResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_RESPONSE_PDU;

        /// <summary>
        /// Size to the end of the packet from AFTER this property using the <see cref="TSOVoltronDistanceToEnd"/> attribute
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ListRoomBodyLength { get; set; }
        /// <summary>
        /// Unsure what this is supposed to be, I think it is the city name
        /// </summary>
        [TSOVoltronString] public string ServiceID { get; set; } = "Blazing Falls";
        /// <summary>
        /// The amount of room objects added to this list
        /// </summary>
        [TSOVoltronValue(TSOVoltronValueTypes.BigEndian)] public uint Amount { get; set; } = 0;
        
        /// <summary>
        /// Rooms that should be sent to the client
        /// </summary>
        public TSORoomInfoStruct[] Rooms { get; set; } = new TSORoomInfoStruct[0];

        /// <summary>
        /// Creates a new <see cref="TSOListRoomsResponsePDU"/> that sends a list of <see cref="TSORoomInfoStruct"/> objects to the client
        /// </summary>
        /// <param name="Rooms"></param>
        public TSOListRoomsResponsePDU(params TSORoomInfoStruct[] Rooms) : base()
        {
            Amount = (uint)Rooms.Length;
            this.Rooms = Rooms;

            MakeBodyFromProperties();
        }

        public TSOListRoomsResponsePDU() : base() { }
    }
}

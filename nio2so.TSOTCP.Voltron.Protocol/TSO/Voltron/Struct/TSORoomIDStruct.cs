using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    /// <summary>
    /// A <see cref="ITSONumeralStringStruct"/> that contains the <see cref="LotPhoneNumber"/> and <see cref="RoomName"/>
    /// </summary>
    public record TSORoomIDStruct : ITSONumeralStringStruct
    {
        public const string NOHOST = "no host", BADROOMNAME = "bad roomname";

        /// <summary>
        /// Creates a blank <see cref="TSORoomIDStruct"/>
        /// </summary>
        public TSORoomIDStruct() {
            LotPhoneNumber = "";
            RoomName = "";
        }

        public TSORoomIDStruct(string lotPhoneNumber, string roomName) : this()
        {
            LotPhoneNumber = lotPhoneNumber;
            RoomName = roomName;
        }

        /// <summary>    
        /// <c>Maps to m_hostName</c><para/>
        /// Follows format: "XXXXXXX" which is given from the <see cref="TSOBuyLotByAvatarIDRequest.LotPhoneNumber"/> property
        /// <para/>Is a unique id for the given cell in the world map
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string LotPhoneNumber { get; set; }
        /// <summary>
        /// <c>Maps to m_roomName</c> The name of the room
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string RoomName { get; set; }


        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public static TSORoomIDStruct Error => new TSORoomIDStruct(NOHOST, BADROOMNAME);
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public static TSORoomIDStruct Blank => new TSORoomIDStruct();

        string ITSONumeralStringStruct.IDString { get => LotPhoneNumber; set => LotPhoneNumber = value; }
        string ITSONumeralStringStruct.NameString { get => RoomName; set => RoomName = value; }
    }
}

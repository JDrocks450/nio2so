using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Struct
{
    /// <summary>
    /// A <see cref="ITSONumeralStringStruct"/> that contains the <see cref="HouseID"/> and <see cref="RoomName"/>
    /// </summary>
    public record TSORoomIDStruct : ITSONumeralStringStruct
    {
        public const string BADROOMNAME = "bad roomname";
        public const uint NOHOST = 0;

        /// <summary>
        /// Creates a blank <see cref="TSORoomIDStruct"/>
        /// </summary>
        public TSORoomIDStruct() {
            HouseID = "";
            RoomName = "";
        }

        public TSORoomIDStruct(uint houseID, string roomName) : this()
        {
            ((ITSONumeralStringStruct)this).NumericID = houseID;            
            RoomName = roomName;
        }

        /// <summary>    
        /// <c>Maps to m_hostName</c><para/>
        /// Follows format: "XXXXXXX" which is given from the <see cref="TSOBuyLotByAvatarIDRequest.HouseIDString"/> property
        /// <para/>Is a unique id for the given cell in the world map
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string HouseID { get; set; }
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

        /// <summary>
        /// This needs to be blank, these cannot have any format specifier
        /// </summary>
        string ITSONumeralStringStruct.FormatSpecifier => "";
        string ITSONumeralStringStruct.IDString { get => HouseID; set => HouseID = value; }
        string ITSONumeralStringStruct.NameString { get => RoomName; set => RoomName = value; }
    }
}

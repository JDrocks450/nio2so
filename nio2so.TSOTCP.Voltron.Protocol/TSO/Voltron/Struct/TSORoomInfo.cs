using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    /// <summary>
    /// An object that has String1 and String2
    /// </summary>
    public interface ITSORoomStringPackStruct
    {
        public string String1 { get; }
        public string String2 { get; }
    }

    /// <summary>
    /// Two strings one after another
    /// </summary>
    /// <param name="String1"></param>
    /// <param name="String2"></param>
    public record TSORoomStringPackStruct(
        [property: TSOVoltronString(TSOVoltronValueTypes.Pascal)] string String1,
        [property: TSOVoltronString(TSOVoltronValueTypes.Pascal)] string String2)
        : ITSORoomStringPackStruct;

    /// <summary>
    /// A <see cref="ITSORoomStringPackStruct"/> that contains the AvatarID and AvatarName
    /// </summary>
    public record TSORoomAvatarInformationStringPackStruct : ITSORoomStringPackStruct
    {
        /// <summary>
        /// Follows format: "__[AvatarID]" where the first two character are seemingly ignored.
        /// <para/>Nio2so would follow this format: "A 1337" where 1337 is the avatar ID
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string AvatarIDString { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string AvatarName { get; set; }

        public TSORoomAvatarInformationStringPackStruct(uint AvatarID, string AvatarName)
        {
            AvatarIDString = TSOAriesIDStruct.FormatIDString(AvatarID);
            this.AvatarName = AvatarName;
        }

        string ITSORoomStringPackStruct.String1 => AvatarIDString;
        string ITSORoomStringPackStruct.String2 => AvatarName;

        [IgnoreDataMember]
        public static TSORoomAvatarInformationStringPackStruct Default => new TSORoomAvatarInformationStringPackStruct(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName);
    }
    /// <summary>
    /// A <see cref="ITSORoomStringPackStruct"/> that contains the <see cref="LotPhoneNumber"/> and <see cref="RoomName"/>
    /// </summary>
    public record TSORoomLotInformationStringPackStruct : ITSORoomStringPackStruct
    {
        public const string NOHOST = "no host", BADROOMNAME = "bad roomname";

        public TSORoomLotInformationStringPackStruct(string lotPhoneNumber, string roomName)
        {
            LotPhoneNumber = lotPhoneNumber;
            RoomName = roomName;
        }

        /// <summary>        
        /// Follows format: "XXXXXXX" which is given from the <see cref="TSOBuyLotByAvatarIDRequest.LotPhoneNumber"/> property
        /// <para/>Is a unique id for the given cell in the world map
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string LotPhoneNumber { get; set; }
        /// <summary>
        /// The name of the room
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string RoomName { get; set; }        

        string ITSORoomStringPackStruct.String1 => LotPhoneNumber;
        string ITSORoomStringPackStruct.String2 => RoomName;

        [IgnoreDataMember]
        public static TSORoomLotInformationStringPackStruct Error => new TSORoomLotInformationStringPackStruct(NOHOST, BADROOMNAME);
    }

    /// <summary>
    /// The structure of an m_RoomInfo structure in Voltron. Describes the settings for a given online room
    /// </summary>
    public record TSORoomInfo
    {        
        public const uint MAX_OCCUPANTS = 10;

        public TSORoomInfo()
        {
        }

        public TSORoomInfo(TSORoomLotInformationStringPackStruct roomLocationInfo,
                           TSORoomAvatarInformationStringPackStruct ownerVector,
                           uint currentOccupants,
                           TSORoomAvatarInformationStringPackStruct? roomHostInformation = default,
                           uint maxOccupants = MAX_OCCUPANTS,
                           bool isLocked = false)                           
        {
            if (roomHostInformation == default) roomHostInformation = ownerVector;
            RoomLocationInfo = RoomLocationInfo2 = roomLocationInfo;
            OwnerVector = ownerVector;
            RoomHostInformation = roomHostInformation;
            CurrentOccupants = currentOccupants;
            MaxOccupants = maxOccupants;
            IsLocked = isLocked;
            RoomHostInformation = roomHostInformation;
        }

        [IgnoreDataMember]
        public static TSORoomInfo DebugSettings => new TSORoomInfo()
        {
            RoomLocationInfo = new(TestingConstraints.MyHousePhoneNumber,TestingConstraints.MyHouseName),            
            OwnerVector = TSORoomAvatarInformationStringPackStruct.Default,
            RoomHostInformation = TSORoomAvatarInformationStringPackStruct.Default,
            CurrentOccupants = 0x01,
        };
        
        public TSORoomLotInformationStringPackStruct RoomLocationInfo { get; set; } = TSORoomLotInformationStringPackStruct.Error;
        /// <summary>
        /// Unsure what this does
        /// </summary>
        public bool Bool1 { get; set; } = true;
        public TSORoomAvatarInformationStringPackStruct OwnerVector { get; set; } = new(0,"");
        [TSOVoltronString] public TSORoomLotInformationStringPackStruct RoomLocationInfo2 { get; set; } = TSORoomLotInformationStringPackStruct.Error;
        /// <summary>
        /// The amount of people currently in this room
        /// </summary>
        public uint CurrentOccupants { get; set; } = 0x0;
        /// <summary>
        /// The maximum amount of people in the room
        /// </summary>
        public uint MaxOccupants { get; set; } = MAX_OCCUPANTS;
        /// <summary>
        /// The lot is locked to outside guests
        /// <para/>HousePage will show (and lot is not joinable):
        /// <code>Online And Locked</code>
        /// </summary>
        public bool IsLocked { get; set; } = false;
        /// <summary>
        /// Unsure what this does
        /// </summary>
        public bool Bool3 { get; set; } = true;

        /// <summary>
        /// Unsure what this is currently used for.
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string String1 { get; set; } = "G";
        public ushort Arg4 { get; set; } = 0x0001;
        /// <summary>
        /// Avatar information for seemingly the host of the room? Unsure of this.
        /// </summary>
        public TSORoomAvatarInformationStringPackStruct RoomHostInformation { get; set; } = new(0, "");
        public byte DataStartByte3 { get; set; } = 0x01;
        public ushort Arg5 { get; set; } = 0x01;
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public TSORoomStringPackStruct StringPack1 { get; set; } = new("J", "K");
        public byte DataStartByte4 { get; set; } = 0x01;
        public ushort Arg6 { get; set; } = 0x01;
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public TSORoomStringPackStruct StringPack2 { get; set; } = new("L", "M");
        public byte EndByte { get; set; } = 0x01;
    }
}

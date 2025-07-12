using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
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
        /// <c>Maps to m_hostName</c><para/>
        /// Follows format: "XXXXXXX" which is given from the <see cref="TSOBuyLotByAvatarIDRequest.LotPhoneNumber"/> property
        /// <para/>Is a unique id for the given cell in the world map
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string LotPhoneNumber { get; set; }
        /// <summary>
        /// <c>Maps to m_roomName</c> The name of the room
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
    public record TSORoomInfoStruct
    {        
        public const uint MAX_OCCUPANTS = 10;

        public TSORoomInfoStruct()
        {
        }

        public TSORoomInfoStruct(TSORoomLotInformationStringPackStruct roomLocationInfo,
                           TSOAriesIDStruct ownerVector,
                           uint currentOccupants,                           
                           uint maxOccupants = MAX_OCCUPANTS,
                           bool isLocked = false,
                           params TSOAriesIDStruct[] roomHostInformation)                           
        {            
            RoomLocationInfo = StageID = roomLocationInfo;
            OwnerVector = ownerVector;
            AdminList = roomHostInformation;
            CurrentOccupants = currentOccupants;
            MaxOccupants = maxOccupants;
            IsLocked = isLocked;
        }

        [IgnoreDataMember]        
        public static TSORoomInfoStruct DebugSettings => new TSORoomInfoStruct()
        {
            RoomLocationInfo = new(TestingConstraints.MyHousePhoneNumber,TestingConstraints.MyHouseName),            
            OwnerVector = new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName),
            AdminList = [new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName)],
            CurrentOccupants = 0x01,
        };

        [IgnoreDataMember]
        public static TSORoomInfoStruct NoRoom => new TSORoomInfoStruct(new("no host", "bad roomname"), new TSOAriesIDStruct("", ""),0);

        public TSORoomLotInformationStringPackStruct RoomLocationInfo { get; set; } = TSORoomLotInformationStringPackStruct.Error;
        /// <summary>
        /// <c>Maps to m_nameLocked</c> and dictates if the name can be changed
        /// </summary>
        public bool NameLocked { get; set; } = true;
        /// <summary>
        /// <c>Maps to m_ownerID</c>
        /// </summary>
        public TSOAriesIDStruct OwnerVector { get; set; } = new(0,"");
        /// <summary>
        /// <c>Maps to m_stageID</c> Should point to the lot this room is taking place at
        /// </summary>
        [TSOVoltronString] public TSORoomLotInformationStringPackStruct StageID { get; set; } = TSORoomLotInformationStringPackStruct.Error;
        /// <summary>
        /// The amount of people currently in this room
        /// </summary>
        public uint CurrentOccupants { get; set; } = 0x0;
        /// <summary>
        /// The maximum amount of people that can be in the room
        /// </summary>
        public uint MaxOccupants { get; set; } = MAX_OCCUPANTS;
        /// <summary>
        /// <c>Maps to s_m_pwdRequired</c>
        /// The lot is locked to outside guests
        /// <para/>HousePage will show (and lot is not joinable):
        /// <code>Online And Locked</code>
        /// </summary>
        public bool IsLocked { get; set; } = false;
        /// <summary>
        /// Unsure what this does
        /// </summary>
        public byte RoomType { get; set; } = 0x01;

        /// <summary>
        /// Not sure what groups mean right now but it is the ID of the group
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string GroupID { get; set; } = "G";

        /// <summary>
        /// <inheritdoc cref="TSOVoltronArrayLength"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(AdminList))]
        public ushort AdminListCount { get; set; }
        /// <summary>
        /// A list of Admin <see cref="TSOAriesIDStruct"/> in this room
        /// </summary>
        public TSOAriesIDStruct[] AdminList { get; set; } = new TSOAriesIDStruct[0];
        public byte DataStartByte3 { get; set; } = 0x01;
        /// <summary>
        /// <inheritdoc cref="AdminListCount"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(AdmitList))]
        public ushort AdmitListCount { get; set; }
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public TSOAriesIDStruct[] AdmitList { get; set; } = new TSOAriesIDStruct[0];
        public byte DataStartByte4 { get; set; } = 0x01;
        /// <summary>
        /// <inheritdoc cref="AdminListCount"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(DenyList))]
        public ushort DenyListCount { get; set; }
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public TSOAriesIDStruct[] DenyList { get; set; } = new TSOAriesIDStruct[0];
        public byte EndByte { get; set; } = 0x01;
    }
}

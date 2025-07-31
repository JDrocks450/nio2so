using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{

    /// <summary>
    /// The structure of an m_RoomInfo structure in Voltron. Describes the settings for a given online room
    /// </summary>
    public record TSORoomInfoStruct
    {        
        public const uint MAX_OCCUPANTS = RoomProtocol.MAX_OCCUPANTS;

        public TSORoomInfoStruct()
        {
        }

        public TSORoomInfoStruct(TSORoomIDStruct roomLocationInfo,
                           TSOAriesIDStruct ownerVector,
                           uint currentOccupants,                           
                           uint maxOccupants = MAX_OCCUPANTS,
                           bool isLocked = false,
                           params TSOAriesIDStruct[] AdminList)                           
        {            
            RoomLocationInfo = StageID = roomLocationInfo;
            OwnerVector = ownerVector;
            if (AdminList.Any())
            {
                this.AdminList = AdminList;
                AdminListEnabled = true;
            }
            CurrentOccupants = currentOccupants;
            MaxOccupants = maxOccupants;
            IsLocked = isLocked;
        }

        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public static TSORoomInfoStruct NoRoom => new TSORoomInfoStruct(TSORoomIDStruct.Blank, new TSOAriesIDStruct("", ""),0);

        public TSORoomIDStruct RoomLocationInfo { get; set; } = TSORoomIDStruct.Blank;
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
        public TSORoomIDStruct StageID { get; set; } = TSORoomIDStruct.Blank;
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
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string GroupID { get; set; } = "BASIC_GROUP";

        /// <summary>
        /// <inheritdoc cref="TSOVoltronArrayLength"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(AdminList))]
        public ushort AdminListCount { get; set; }
        /// <summary>
        /// A list of Admin <see cref="TSOAriesIDStruct"/> in this room
        /// </summary>
        public TSOAriesIDStruct[] AdminList { get; set; } = new TSOAriesIDStruct[0];
        public bool AdminListEnabled { get; set; } = false;
        /// <summary>
        /// <inheritdoc cref="AdminListCount"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(AdmitList))]
        public ushort AdmitListCount { get; set; }
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public TSOAriesIDStruct[] AdmitList { get; set; } = new TSOAriesIDStruct[0];
        public bool AdmitListEnabled { get; set; } = false;
        /// <summary>
        /// <inheritdoc cref="AdminListCount"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(DenyList))]
        public ushort DenyListCount { get; set; }
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public TSOAriesIDStruct[] DenyList { get; set; } = new TSOAriesIDStruct[0];
        public bool DenyListEnabled { get; set; } = false;
    }
}

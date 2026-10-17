using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol
{

    /// <summary>
    /// A kMSG is used to invoke a Regulator to change its state or respond to a stimulus.
    /// <para>For example, the DBServiceClientD will use the <see cref="kDBServiceRequestMsg"/> to send data.
    /// and uses the <see cref="kDBServiceResponseMsg"/> to be notified when to receive data.</para>
    /// </summary>
    public enum TSO_NewImproved_VoltronPacketTypes_kMSGs : uint
    {
        /// <summary>
        /// <see cref="TSODBRequestWrapper"/> PDUs that are intended as Requests
        /// </summary>
        kDBServiceRequestMsg = 0x3BF82D4E,
        /// <summary>
        /// <see cref="TSODBRequestWrapper"/> PDUs that are intended as Responses
        /// </summary>
        kDBServiceResponseMsg = 0xDBF301A9,
        /// <summary>
        /// <see cref="TSOVoltronDatablobContent"/> PDUs that are intended as Responses
        /// </summary>
        kSimResponseMsg = 0x7B6F2221,
        /// <summary>
        /// <see cref="TSOVoltronDatablobContent"/> PDUs that are intended as Requests
        /// </summary>
        kSimRequestMsg = 0x1B6F221B,
    }

    public enum TSO_NewImproved_TSOData_StructIDs : uint
    {
        /// <summary>
        /// ID of the Level Two Struct Avatar
        /// </summary>
        Avatar = 0x05600332, 
        /// <summary>
        /// ID of the Level Two Struct Lot
        /// </summary>
        Lot = 0x7A6CECEE, 
    }

    public enum TSO_NewImproved_GZMSGIDs : uint
    {
        GZMSGID_cITSOApprovedTopicUpdate = 0xA97360C5,
    }

    public enum TSO_NewImproved_TSOValueBase_CLSIDs : uint
    {
        cTSOTopicUpdateMessage = 0x09736027,

        cTSOValueVector_Uint8 = 0x097608AB,
        cTSOValueVector_Sint8 = 0x097608AF,
        cTSOValueVector_Uint16 = 0x097608B3,
        cTSOValueVector_Sint16 = 0x097608B6,

        cTSODataTransportBuffer = 0x0A2C6585,

        cTSOTopic = 0x29739B14,
        cTSOTopicUpdateErrorMessage = 0x2A404946,

        cTSODataDefinitionContainer = 0x496A78BC,

        cTSOValue_Bool = 0x696D1183,
        cTSOValue_Uint32 = 0x696D1189,
        cTSOValue_Uint64 = 0x69D3E3DB,

        cTSOValue_Sint32 = 0x896D1196,
        cTSOValueMap_Property = 0x896D11A2,
        cTSOValue_String = 0x896D1688,

        cTSOValueBVector = 0x89738492,
        cTSOValueVector_Uint32 = 0x89738496,
        cTSOValueVector_Sint32 = 0x8973849A,
        cTSOValueVector_String = 0x8973849E,

        cTSOProperty = 0x89739A79, // Property

        cTSOValue_Sint64 = 0x89D3E3EF,
        cTSOValueMap_Uint64 = 0x89D3E40E,

        cTSOValueMap_Uint32 = 0xA96E38A0,
        cTSOValueMap_Sint32 = 0xA96E38A8,
        cTSOValueMap_String = 0xA96E38AC,

        cTSOValue_Property = 0xA96E7E5B, // ValueProperty

        cTSODataService = 0xA97353EE,
        cTSOValueVector_Property = 0xA97384A3,
        cTSODataServiceClient = 0xA975FA6E,

        cTSOValueMap_Unknown = 0xA99AF3A8,
        cTSOValueVector_Unknown = 0xA99AF3AC,
        cTSOValue_Unknown = 0xA99AF3B7,

        cTSOValueMap_Sint64 = 0xA9D3E412,
        cTSOValueVector_Uint64 = 0xA9D3E428,
        cTSOValueVector_Sint64 = 0xA9D3E42D,

        cTSOValue_Uint8 = 0xC976087C,
        cTSOValueMap_Bool = 0xC97757F5,

        cTSOValue_Sint8 = 0xE976088A,
        cTSOValue_Uint16 = 0xE9760891,
        cTSOValue_Sint16 = 0xE9760897,

        cTSOValueMap_Uint8 = 0xE976089F,
        cTSOValueMap_Sint8 = 0xE97608A2,
        cTSOValueMap_Uint16 = 0xE97608A5,
        cTSOValueMap_Sint16 = 0xE97608A8
    }


    public enum TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs : uint
    {
        /// <summary>
        /// A standard message, the most common type of message found in TSO
        /// </summary>
        cTSONetMessageStandard = 0x125194E5,
        cTSONetMessageStream = 0x125194F5,
        cTSOAvatarCreationRequest = 0x3EA44787,
        cTSOInterdictor = 0xAA3ECCB3,
        cTSOInterdictionPass = 0xAA5FA4D8,
        cTSOInterdictionPassAndLog = 0xCA5FA4E0,
        cTSOInterdictionDrop = 0xCA5FA4E3,
        cTSOInterdictionDropAndLog = 0xCA5FA4EB,
        cTSONetMessageEnvelope = 0xAA7B191E,
        cTSOChannelMessageEnvelope = 0x2A7B4E6A,
        cTSODeadStream = 0x0A9D7E3A,
        cTSOTopicUpdateMessage = 0x09736027,
        cTSODataTransportBuffer = 0x0A2C6585,
        cTSOTopicUpdateErrorMessage = 0x2A404946
    }
    internal enum TSO_NewImproved_VoltronPacketTypes : ushort
    {
        AlertHandledPDU = 0x0001,
        AlertMsgPDU = 0x0002,
        AlertMsgResponsePDU = 0x0003,
        AnnouncementMsgResponsePDU = 0x0004,
        AnnouncementMsgPDU = 0x0005,
        ClientByePDU = 0x0006,
        ServerByePDU = 0x0007,
        ChatMsgFailedPDU = 0x0008,
        ChatMsgPDU = 0x0009,
        ClientOnlinePDU = 0x000A,
        CreateAndJoinRoomFailedPDU = 0x000B,
        CreateAndJoinRoomPDU = 0x000C,
        CreateRoomPDU = 0x000D,
        CreateRoomResponsePDU = 0x000E,
        DestroyRoomPDU = 0x000F,
        DestroyRoomResponsePDU = 0x0010,
        DetachFromRoomFailedPDU = 0x0011,
        DetachFromRoomPDU = 0x0012,
        EjectOccupantPDU = 0x0013,
        EjectOccupantResponsePDU = 0x0014,
        ErrorPDU = 0x0015,
        ExitRoomFailedPDU = 0x0016,
        ExitRoomPDU = 0x0017,
        FindPlayerPDU = 0x0018,
        FindPlayerResponsePDU = 0x0019,
        FlashMsgResponsePDU = 0x001A,
        FlashMsgPDU = 0x001B,
        HandleAlertPDU = 0x001C,
        HostOfflinePDU = 0x001D,
        HostOnlinePDU = 0x001E,
        InvitationMsgResponsePDU = 0x001F,
        InvitationMsgPDU = 0x0020,
        JoinPlayerFailedPDU = 0x0021,
        JoinPlayerPDU = 0x0022,
        JoinRoomFailedPDU = 0x0023,
        JoinRoomPDU = 0x0024,
        ListOccupantsPDU = 0x0025,
        ListOccupantsResponsePDU = 0x0026,
        ListRoomsPDU = 0x0027,
        ListRoomsResponsePDU = 0x0028,
        LogEventPDU = 0x0029,
        LogEventResponsePDU = 0x002A,
        MessageLostPDU = 0x002B,
        OccupantArrivedPDU = 0x002C,
        OccupantDepartedPDU = 0x002D,
        ReadProfilePDU = 0x002E,
        ReadProfileResponsePDU = 0x002F,
        ReleaseProfilePDU = 0x0030,
        ReleaseProfileResponsePDU = 0x0031,
        SetAcceptAlertsPDU = 0x0032,
        SetAcceptAlertsResponsePDU = 0x0033,
        SetIgnoreListPDU = 0x0034,
        SetIgnoreListResponsePDU = 0x0035,
        SetInvinciblePDU = 0x0036,
        SetInvincibleResponsePDU = 0x0037,
        SetInvisiblePDU = 0x0038,
        SetInvisibleResponsePDU = 0x0039,
        SetRoomNamePDU = 0x003A,
        SetRoomNameResponsePDU = 0x003B,
        UpdateOccupantsPDU = 0x003C,
        UpdatePlayerPDU = 0x003D,
        UpdateProfilePDU = 0x003E,
        UpdateRoomPDU = 0x003F,
        PLACEHOLDER1 = 0x0040,
        PLACEHOLDER2 = 0x0041,

        YankPlayerFailedPDU = 0x0042,
        YankPlayerPDU = 0x0043,
        SetAcceptFlashesPDU = 0x0044,
        SetAcceptFlashesResponsePDU = 0x0045,
        SplitBufferPDU = 0x0046,
        ActionRoomNamePDU = 0x0047,
        ActionRoomNameResponsePDU = 0x0048,
        NotifyRoomActionedPDU = 0x0049,
        ModifyProfilePDU = 0x004A,
        ModifyProfileResponsePDU = 0x004B,
        ListBBSFoldersPDU = 0x004C,
        ListBBSFoldersResponsePDU = 0x004D,
        GetBBSMessageListPDU = 0x004E,
        GetBBSMessageListResponsePDU = 0x004F,
        PostBBSMessagePDU = 0x0050,
        PostBBSReplyPDU = 0x0051,
        PostBBSMessageResponsePDU = 0x0052,
        GetMPSMessagesPDU = 0x0053,
        GetMPSMessagesResponsePDU = 0x0054,
        DeleteMPSMessagePDU = 0x0055,
        DeleteMPSMessageResponsePDU = 0x0056,
        BBSMessageDataPDU = 0x0057,
        UpdateRoomAdminListPDU = 0x0058,
        GetRoomAdminListPDU = 0x0059,
        GetRoomAdminListResponsePDU = 0x005A,
        GroupInfoRequestPDU = 0x005B,
        GroupInfoResponsePDU = 0x005C,
        GroupAdminRequestPDU = 0x005D,
        GroupAdminResponsePDU = 0x005E,
        GroupMembershipRequestPDU = 0x005F,
        GroupMembershipResponsePDU = 0x0060,
        FlashGroupPDU = 0x0061,
        FlashGroupResponsePDU = 0x0062,
        UpdateGroupMemberPDU = 0x0063,
        UpdateGroupMemberResponsePDU = 0x0064,
        UpdateGroupAdminPDU = 0x0065,
        UpdateGroupAdminResponsePDU = 0x0066,
        ListGroupsPDU = 0x0067,
        ListGroupsResponsePDU = 0x0068,
        ListJoinedGroupsPDU = 0x0069,
        ListJoinedGroupsResponsePDU = 0x006A,
        GpsChatPDU = 0x006B,
        GpsChatResponsePDU = 0x006C,
        PetitionStatusUpdatePDU = 0x006D,
        LogGPSPetitionPDU = 0x006E,
        LogGPSPetitionResponsePDU = 0x006F,
        List20RoomsPDU = 0x0070,
        List20RoomsResponsePDU = 0x0071,
        UpdateIgnoreListPDU = 0x0072,
        ResetWatchdogPDU = 0x0073,
        ResetWatchdogResponsePDU = 0x0074,

        // The Sims Online PDUs
        BroadcastDataBlobPDU = 0x2710,
        TransmitDataBlobPDU = 0x2711,
        DBRequestWrapperPDU = 0x2712,
        TransmitCreateAvatarNotificationPDU = 0x2713,
        BC_PlayerLoginEventPDU = 0x2715,
        BC_PlayerLogoutEventPDU = 0x2716,
        RoomserverUserlistPDU = 0x2718,
        LotEntryRequestPDU = 0x2719,
        ClientConfigPDU = 0x271A,
        KickoutRoommatePDU = 0x271C,
        GenericFlashPDU = 0x271D,
        GenericFlashRequestPDU = 0x271E,
        GenericFlashResponsePDU = 0x271F,
        TransmitGenericGDMPDU = 0x2722,
        EjectAvatarPDU = 0x2723,
        TestPDU = 0x2724,
        HouseSimConstraintsPDU = 0x2725,
        HouseSimConstraintsResponsePDU = 0x2726,
        LoadHouseResponsePDU = 0x2728,
        ComponentVersionRequestPDU = 0x2729,
        ComponentVersionResponsePDU = 0x272A,
        InviteRoommatePDU = 0x272B,
        RoommateInvitationAnswerPDU = 0x272C,
        RoommateGDMPDU = 0x272D,
        HSB_ShutdownSimulatorPDU = 0x272E,
        RoommateGDMResponsePDU = 0x272F,
        RSGZWrapperPDU = 0x2730,
        AvatarHasNewLotIDPDU = 0x2731,
        CheatPDU = 0x2733,
        DataServiceWrapperPDU = 0x2734,
        CsrEjectAvatarPDU = 0x2735,
        CsrEjectAvatarResponsePDU = 0x2736,
        cTSONetMessagePDU = 0x2737,
        LogCsrActionPDU = 0x2738,
        LogAvatarActionPDU = 0x2739
    }
}

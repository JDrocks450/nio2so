using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PlayTest.Protocol
{
    /// <summary>
    /// A kMSG is used to invoke a Regulator to change its state or respond to a stimulus.
    /// <para>For example, the DBServiceClientD will use the <see cref="kDBServiceRequestMsg"/> to send data.
    /// and uses the <see cref="kDBServiceResponseMsg"/> to be notified when to receive data.</para>
    /// </summary>
    public enum TSO_PlayTest_kMSGs : uint
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
    public enum TSO_PlayTest_MsgCLSIDs : uint
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
    internal enum TSO_PlayTest_VoltronPacketTypes : ushort
    {
        AlertHandledPDU = 0x0001,
        AlertMsgPDU = 0x0002,
        AlertMsgResponsePDU = 0x0003,
        AnnouncementMsgResponsePDU = 0x0004,
        AnnouncementMsgPDU = 0x0005,
        ClientByePDU = 0x0006,
        //ServerByePDU = 0x0007,
        ChatMsgFailedPDU = 0x0007,
        ChatMsgPDU = 0x0008,
        ClientOnlinePDU = 0x0009,
        CreateAndJoinRoomFailedPDU = 0x000A,
        CreateAndJoinRoomPDU = 0x000B,
        CreateRoomPDU = 0x000C,
        CreateRoomResponsePDU = 0x000D,
        DestroyRoomPDU = 0x000E,
        DestroyRoomResponsePDU = 0x000F,
        DetachFromRoomFailedPDU = 0x0010,
        DetachFromRoomPDU = 0x0011,
        EjectOccupantPDU = 0x0012,
        EjectOccupantResponsePDU = 0x0013,
        ErrorPDU = 0x0014,
        ExitRoomFailedPDU = 0x0015,
        ExitRoomPDU = 0x0016,
        FindPlayerPDU = 0x0017,
        FindPlayerResponsePDU = 0x0018,
        FlashMsgResponsePDU = 0x0019,
        FlashMsgPDU = 0x001A,
        HandleAlertPDU = 0x001B,
        HostOfflinePDU = 0x001C,
        HostOnlinePDU = 0x001D,
        InvitationMsgResponsePDU = 0x001E,
        InvitationMsgPDU = 0x001F,
        JoinPlayerFailedPDU = 0x0020,
        JoinPlayerPDU = 0x0021,
        JoinRoomFailedPDU = 0x0022,
        JoinRoomPDU = 0x0023,
        ListOccupantsPDU = 0x0024,
        ListOccupantsResponsePDU = 0x0025,
        ListRoomsPDU = 0x0026,
        ListRoomsResponsePDU = 0x0027,
        LogEventPDU = 0x0028,
        LogEventResponsePDU = 0x0029,
        MessageLostPDU = 0x002A,
        OccupantArrivedPDU = 0x002B,
        OccupantDepartedPDU = 0x002C,
        ReadProfilePDU = 0x002D,
        ReadProfileResponsePDU = 0x002E,
        ReleaseProfilePDU = 0x002F,
        ReleaseProfileResponsePDU = 0x0030,
        SetAcceptAlertsPDU = 0x0031,
        SetAcceptAlertsResponsePDU = 0x0032,
        SetIgnoreListPDU = 0x0033,
        SetIgnoreListResponsePDU = 0x0034,
        SetInvinciblePDU = 0x0035,
        SetInvincibleResponsePDU = 0x0036,
        SetInvisiblePDU = 0x0037,
        SetInvisibleResponsePDU = 0x0038,
        SetRoomNamePDU = 0x0039,
        SetRoomNameResponsePDU = 0x003A,
        UpdateOccupantsPDU = 0x003B,
        UpdatePlayerPDU = 0x003C,
        UpdateProfilePDU = 0x003D,
        UpdateRoomPDU = 0x003E,
        PLACEHOLDER1 = 0x3F,
        PLACEHOLDER2 = 0x40,

        YankPlayerFailedPDU = 0x0041,
        YankPlayerPDU = 0x0041,
        SetAcceptFlashesPDU = 0x0042,
        SetAcceptFlashesResponsePDU = 0x0043,
        SplitBufferPDU = 0x0044,
        ActionRoomNamePDU = 0x0045,
        ActionRoomNameResponsePDU = 0x0046,
        NotifyRoomActionedPDU = 0x0047,
        ModifyProfilePDU = 0x0048,
        ModifyProfileResponsePDU = 0x0049,
        ListBBSFoldersPDU = 0x004A,
        ListBBSFoldersResponsePDU = 0x004B,
        GetBBSMessageListPDU = 0x004C,
        GetBBSMessageListResponsePDU = 0x004D,
        PostBBSMessagePDU = 0x004E,
        PostBBSReplyPDU = 0x004F,
        PostBBSMessageResponsePDU = 0x0050,
        GetMPSMessagesPDU = 0x0051,
        GetMPSMessagesResponsePDU = 0x0052,
        DeleteMPSMessagePDU = 0x0053,
        DeleteMPSMessageResponsePDU = 0x0054,
        BBSMessageDataPDU = 0x0055,
        UpdateRoomAdminListPDU = 0x0056,
        GetRoomAdminListPDU = 0x0057,
        GetRoomAdminListResponsePDU = 0x0058,
        GroupInfoRequestPDU = 0x0059,
        GroupInfoResponsePDU = 0x005A,
        GroupAdminRequestPDU = 0x005B,
        GroupAdminResponsePDU = 0x005C,
        GroupMembershipRequestPDU = 0x005D,
        GroupMembershipResponsePDU = 0x005E,
        FlashGroupPDU = 0x005F,
        FlashGroupResponsePDU = 0x0060,
        UpdateGroupMemberPDU = 0x0061,
        UpdateGroupMemberResponsePDU = 0x0062,
        UpdateGroupAdminPDU = 0x0063,
        UpdateGroupAdminResponsePDU = 0x0064,
        ListGroupsPDU = 0x0065,
        ListGroupsResponsePDU = 0x0066,
        ListJoinedGroupsPDU = 0x0067,
        ListJoinedGroupsResponsePDU = 0x0068,
        GpsChatPDU = 0x0069,
        GpsChatResponsePDU = 0x006A,
        PetitionStatusUpdatePDU = 0x006B,
        LogGPSPetitionPDU = 0x006C,
        LogGPSPetitionResponsePDU = 0x006D,
        List20RoomsPDU = 0x006E,
        List20RoomsResponsePDU = 0x006F,
        UpdateIgnoreListPDU = 0x0070,
        ResetWatchdogPDU = 0x0071,
        ResetWatchdogResponsePDU = 0x0072,

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

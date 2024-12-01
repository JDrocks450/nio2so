namespace nio2so.TSOTCP.City.TSO.Voltron
{
    /// <summary>
    /// For discovering more PDUs, please refer to: http://niotso.org/files/prealpha_pdu_tables.txt
    /// </summary>
    public enum TSO_PreAlpha_VoltronPacketTypes : ushort
    {
        UNKNOWN_MESSAGE_ID_PDU = 0x0000,
        ALERT_HANDLED_PDU = 0x0001,
        ALERT_MSG_PDU = 0x0002,
        ALERT_MSG_RESPONSE_PDU = 0x0003,
        ANNOUNCEMENT_MSG_RESPONSE_PDU = 0x0004,
        ANNOUNCEMENT_MSG_PDU = 0x0005,
        BYE_PDU = 0x0006,
        CHAT_MSG_FAILED_PDU = 0x0007,
        CHAT_MSG_PDU = 0x0008,
        CLIENT_ONLINE_PDU = 0x0009,
        CREATE_AND_JOIN_ROOM_FAILED_PDU = 0x000A,
        CREATE_AND_JOIN_ROOM_PDU = 0x000B,
        CREATE_ROOM_PDU = 0x000C,
        CREATE_ROOM_RESPONSE_PDU = 0x000D,
        DESTROY_ROOM_PDU = 0x000E,
        DESTROY_ROOM_RESPONSE_PDU = 0x000F,
        DETACH_FROM_ROOM_FAILED_PDU = 0x0010,
        DETACH_FROM_ROOM_PDU = 0x0011,
        EJECT_OCCUPANT_PDU = 0x0012,
        EJECT_OCCUPANT_RESPONSE_PDU = 0x0013,
        ERROR_PDU = 0x0014,
        EXIT_ROOM_FAILED_PDU = 0x0015,
        EXIT_ROOM_PDU = 0x0016,
        FIND_PLAYER_PDU = 0x0017,
        FIND_PLAYER_RESPONSE_PDU = 0x0018,
        FLASH_MSG_RESPONSE_PDU = 0x0019,
        FLASH_MSG_PDU = 0x001A,
        HANDLE_ALERT_PDU = 0x001B,
        HOST_OFFLINE_PDU = 0x001C,
        HOST_ONLINE_PDU = 0x001D,
        INVITATION_MSG_RESPONSE_PDU = 0x001E,
        INVITATION_MSG_PDU = 0x001F,
        JOIN_PLAYER_FAILED_PDU = 0x0020,
        JOIN_PLAYER_PDU = 0x0021,
        JOIN_ROOM_FAILED_PDU = 0x0022,
        JOIN_ROOM_PDU = 0x0023,
        LIST_OCCUPANTS_PDU = 0x0024,
        LIST_OCCUPANTS_RESPONSE_PDU = 0x0025,
        LIST_ROOMS_PDU = 0x0026,
        LIST_ROOMS_RESPONSE_PDU = 0x0027,
        LOG_EVENT_PDU = 0x0028,
        LOG_EVENT_RESPONSE_PDU = 0x0029,
        MESSAGE_LOST_PDU = 0x002A,
        OCCUPANT_ARRIVED_PDU = 0x002B,
        OCCUPANT_DEPARTED_PDU = 0x002C,
        READ_PROFILE_PDU = 0x002D,
        READ_PROFILE_RESPONSE_PDU = 0x002E,
        RELEASE_PROFILE_PDU = 0x002F,
        RELEASE_PROFILE_RESPONSE_PDU = 0x0030,
        SET_ACCEPT_ALERTS_PDU = 0x0031,
        SET_ACCEPT_ALERTS_RESPONSE_PDU = 0x0032,
        SET_IGNORE_LIST_PDU = 0x0033,
        SET_IGNORE_LIST_RESPONSE_PDU = 0x0034,
        SET_INVINCIBLE_PDU = 0x0035,
        SET_INVINCIBLE_RESPONSE_PDU = 0x0036,
        SET_INVISIBLE_PDU = 0x0037,
        SET_INVISIBLE_RESPONSE_PDU = 0x0038,
        SET_ROOM_NAME_PDU = 0x0039,
        SET_ROOM_NAME_RESPONSE_PDU = 0x003A,
        UPDATE_OCCUPANTS_PDU = 0x003B,
        UPDATE_PLAYER_PDU = 0x003C,
        UPDATE_PROFILE_PDU = 0x003D,
        UPDATE_ROOM_PDU = 0x003E,
        //= 0x003F -
        //= 0x0040 -
        YANK_PLAYER_FAILED_PDU = 0x0041,
        YANK_PLAYER_PDU = 0x0042,
        SET_ACCEPT_FLASHES_PDU = 0x0043,
        SET_ACCEPT_FLASHES_RESPONSE_PDU = 0x0044,
        SPLIT_BUFFER_PDU = 0x0045,
        ACTION_ROOM_NAME_PDU = 0x0046,
        ACTION_ROOM_NAME_RESPONSE_PDU = 0x0047,
        NOTIFY_ROOM_ACTIONED_PDU = 0x0048,
        MODIFY_PROFILE_PDU = 0x0049,
        MODIFY_PROFILE_RESPONSE_PDU = 0x004A,
        LIST_BBS_FOLDERS_PDU = 0x004B,
        LIST_BBS_FOLDERS_RESPONSE_PDU = 0x004C,
        GET_BBS_MESSAGE_LIST_PDU = 0x004D,
        GET_BBS_MESSAGE_LIST_RESPONSE_PDU = 0x004E,
        POST_BBS_MESSAGE_PDU = 0x004F,
        POST_BBS_REPLY_PDU = 0x0050,
        POST_BBS_RESPONSE_PDU = 0x0051,
        GET_MPS_MESSAGES_PDU = 0x0052,
        GET_MPS_MESSAGES_RESPONSE_PDU = 0x0053,
        DELETE_MPS_MESSAGE_PDU = 0x0054,
        DELETE_MPS_MESSAGE_RESPONSE_PDU = 0x0055,
        BBS_MESSAGE_DATA_PDU = 0x0056,
        UPDATE_ROOM_ADMIN_LIST_PDU = 0x0057,
        GET_ROOM_ADMIN_LIST_PDU = 0x0058,
        GET_ROOM_ADMIN_LIST_RESPONSE_PDU = 0x0059,

        //This table is found at TSOVoltronDMServiceD_base+ = 0x73738 in TSO Pre-Alpha
        //(according to TSOVoltronDMServiceD_base+ = 0x47d70, the first entry corresponds to the value  = 0x0080)

        BROADCAST_DATABLOB_PDU = 0x0080,
        TRANSMIT_DATABLOB_PDU = 0x0081,
        DB_REQUEST_WRAPPER_PDU = 0x0082,
        TRANSMIT_CREATEAVATARNOTIFICATION_PDU = 0x0083,
        ROOMSERVER_INITIALIZED_PDU = 0x0084,
        BC_PLAYER_LOGIN_EVENT_PDU = 0x0085,
        BC_PLAYER_LOGOUT_EVENT_PDU = 0x0086,
        BC_PLAYER_DISCONNECT_EVENT_PDU = 0x0087,
        ROOMSERVER_USERLIST_PDU = 0x0088,
        LOT_ENTRY_REQUEST_PDU = 0x0089,
        CLIENT_CONFIG_PDU = 0x008A,
        BC_DELETE_AVATAR = 0x008B,
        KICKOUT_ROOMMATE_PDU = 0x008C,
        BC_ACCESS_PDU = 0x008D,
        //DUPLICATE?
        BC_ACCESS_PDU_2 = 0x008E,
        BC_ACCESS_LIST_PDU = 0x008F,
        BC_HEART_BEAT_PDU = 0x0090,
        BC_CONNECT_PDU = 0x0091,
        BC_VERSION_LIST_PDU = 0x0092,
        TRANSMIT_GENERIC_GDM_PDU = 0x0093,
        EJECT_VISITOR_PDU = 0x0094,
        EJECT_VISITOR_RESPONSE_PDU = 0x0095,
        HOUSE_SIM_CONSTRAINTS_PDU = 0x0096,
        HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU = 0x0097,
        LOAD_HOUSE_PDU = 0x0098,
        LOAD_HOUSE_RESPONSE_PDU = 0x0099,
        COMPONENT_VERSION_REQUEST_PDU = 0x009A,
        COMPONENT_VERSION_RESPONSE_PDU = 0x009B,
        INVITE_ROOMMATE_PDU = 0x009C,
        ROOMMATE_INVITATION_ANSWER_PDU = 0x009D,
        ROOMMATE_GDM_PDU = 0x009E,
        HSB_SHUTDOWN_SIMULATOR_PDU = 0x009F,
        ROOMMATE_GDM_RESPONSE_PDU = 0x00A0,
        RS_GZ_WRAPPER_PDU = 0x00A1,
        AVATAR_HAS_NEW_LOTID_PDU = 0x00A2,
    }
    /// <summary>
    /// <para>For discovering more CLSIDs, please refer to: http://niotso.org/files/prealpha_constants_table.txt</para>
    /// </summary>
    public enum TSO_PreAlpha_DBStructCLSIDs : uint
    {
        cCrDMStandardMessage = 0x125194E5,
        cCrDMTestObject = 0x122A94F2,
        cTSOSerializableStream = 0xDBB9126C,

        //*PROBES

        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.GZPROBEID_cEAS"/>
        /// </summary>
        GZPROBEID_cEAS = 0x1D873D36,
        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.GZPROBEID_cMV"/>
        /// </summary>
        GZPROBEID_cMV = 0x1A873D38,
        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.GZPROBEID_cObjectShopping"/>
        /// </summary>
        GZPROBEID_cObjectShopping = 0xC990F66B,
        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.kGZPROBEID_TWOWAYINTERACTIONOUTCOME"/>
        /// </summary>
        kGZPROBEID_TWOWAYINTERACTIONOUTCOME = 0xE9B547F8,
        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.GZPROBEID_cSAS"/>
        /// </summary>
        GZPROBEID_cSAS = 0xFD864D0E,
        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.GZPROBEID_cMoneyInteraction"/>
        /// </summary>
        GZPROBEID_cMoneyInteraction = 0x09944F96,
        /// <summary>
        /// <see cref="TSO_PreAlpha_GZPROBE.GZPROBEID_cSimLetters"/>
        /// </summary>
        GZPROBEID_cSimLetters = 0x09944F97
    }

    public enum TSO_PreAlpha_GZPROBE : uint
    {
        /// <summary>
        /// This is the Probe for Edit-A-Sim
        /// </summary>
        GZPROBEID_cEAS = TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cEAS,
        /// <summary>
        /// This is the Probe for MapView?
        /// </summary>
        GZPROBEID_cMV = TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cMV,
        /// <summary>
        /// This is the Probe for shopping for objects?
        /// </summary>
        GZPROBEID_cObjectShopping = TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cObjectShopping,
        /// <summary>
        /// This is the Probe for two way interactions
        /// </summary>
        kGZPROBEID_TWOWAYINTERACTIONOUTCOME = TSO_PreAlpha_DBStructCLSIDs.kGZPROBEID_TWOWAYINTERACTIONOUTCOME,
        /// <summary>
        /// This is the Probe for Select-A-Sim
        /// </summary>
        GZPROBEID_cSAS = TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cSAS,
        /// <summary>
        /// This is the Probe for money interactions
        /// </summary>
        GZPROBEID_cMoneyInteraction = TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cMoneyInteraction,
        /// <summary>
        /// This is the Probe for sending letters to sims (Persistent IM?)
        /// </summary>
        GZPROBEID_cSimLetters = TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cSimLetters
    }

    /// <summary>
    /// The Sims Online makes a distinction between Queries, Requests and Responses
    /// <para>Generally, the Query is what the game uses to invoke the DBAppService to make the Request packet.</para>
    /// <para>When responding to a Request, you need to find the accompanying Response CLSID, if applicable.</para>
    /// 
    /// <para>For discovering more CLSIDs, please refer to: http://niotso.org/files/prealpha_constants_table.txt</para>
    /// </summary>
    public enum TSO_PreAlpha_DBActionCLSIDs : uint
    {
        /// <summary>
        /// Asks for a list of Roommates at the given LotID in Data1. <para/>
        /// Structure implementation does not exist but can be read by nio2so
        /// </summary>
        GetRoommateInfoByLotID_Request = 0xFD3338E9,
        /// <summary>
        /// Responds with a <c>TSOGetRoommateInfoByLotIDResponse</c> packet structure
        /// Structure implemented -- not fully understood
        /// </summary>
        GetRoommateInfoByLotID_Response = 0xDD3339EE,
        /// <summary>
        /// Used to request the full Avatar File. It contains the appearance, name, description, personality, skills, etc. of an avatar. <para/>
        /// Implemented and working: <see cref="GetCharBlobByID_Request"/>, <see cref="GetCharBlobByID_Response"/>, <see cref="InsertNewCharBlob_Request"/>
        /// <para/>Structured: <see cref="SetCharBlobByID_Request"/>, <see cref="SetCharBlobByID_Response"/>, <see cref="InsertNewCharBlob_Response"/>
        /// </summary>
        GetCharBlobByID_Request = 0x5BB73FAB,
        /// <summary>
        /// Used to serve the full Avatar File. It contains the appearance, name, description, personality, skills, etc. of an avatar. <para/>
        /// Implemented and working: <see cref="GetCharBlobByID_Request"/>, <see cref="GetCharBlobByID_Response"/>, <see cref="InsertNewCharBlob_Request"/>
        /// <para/>Structured: <see cref="SetCharBlobByID_Request"/>, <see cref="SetCharBlobByID_Response"/>, <see cref="InsertNewCharBlob_Response"/>
        /// </summary>
        GetCharBlobByID_Response = 0x5BB73FE4,
        /// <summary>
        /// Requests to update the database's Avatar File with the new one created.
        /// <para/>Sent from Edit-A-Sim: Complete and working in nio2so
        /// </summary>
        SetCharBlobByID_Request = 0xDBB75B67,
        /// <summary>
        /// Not tested
        /// </summary>
        SetCharBlobByID_Response = 0xDCF17EED,
        /// <summary>
        /// Used to get very sparse info about an Avatar like name and description. <para/>
        /// Complete and working.
        /// </summary>
        GetCharByID_Request = 0x7BAE5079,
        /// <summary>
        /// Used to get very sparse info about an Avatar like name and description. <para/>
        /// Complete and working.
        /// </summary>
        GetCharByID_Response = 0x1BAE532A,
        /// <summary>
        /// Used when the Client is updating the Char data on a specific Avatar.
        /// Payload is the new Char data stream
        /// </summary>
        SetCharByID_Request = 0xBC02858A,
        /// <summary>
        /// Responds with a confirmation code in a <c>TSOSetCharByIDResponse</c> packet.
        /// </summary>
        SetCharByID_Response = 0x1CF17ECB,
        /// <summary>
        /// Used when requesting information on current relationships on a given Avatar.
        /// <para/> This is on a basis of how THIS Avatar feels about other Avatars, supposedly.
        /// <para/> There is a reverse of this Request for getting relationships in terms of how others feel about this Avatar
        /// <para/> Not implemented
        /// </summary>
        GetRelationshipsByID_Request = 0x3BF96A6C,
        /// <summary>
        /// Used when requesting information on current relationships on a given Avatar.
        /// <para/> This is on a basis of how THIS Avatar feels about other Avatars, supposedly.
        /// <para/> There is a reverse of this Request for getting relationships in terms of how others feel about this Avatar
        /// <para/> Not implemented
        /// </summary>
        GetRelationshipsByID_Response = 0x9BF972CB,
        /// <summary>
        /// Gets the list of lot IDs that are added to the World Map
        /// </summary>
        GetLotList_Request = 0x5BEEB701,
        /// <summary>
        /// Returns the list of lot IDs that are added to the World Map
        /// </summary>
        GetLotList_Response = 0xDBEECD65,
        /// <summary>
        /// Not implemented
        /// </summary>
        GetLotByID_Request = 0xFBE96AA3,
        /// <summary>
        /// Requests who is currently the House Leader of a given lot.
        /// </summary>
        GetHouseLeaderByLotID_Request = 0xDD909124,
        /// <summary>
        /// Not implemented
        /// </summary>
        GetHouseLeaderByLotID_Response = 0xBD90911F,
        /// <summary>
        /// Gets the house data for a given LotID.
        /// <para/>
        /// GZCLSID_cDBGetHouseBlobByID_Request
        /// </summary>
        GetHouseBlobByID_Request = 0x5BB8D069,
        /// <summary>
        /// Used when the Client wants to update the data in the DB at the given LotID
        /// </summary>
        SetHouseBlobByID_Request = 0x5BB8DC3C,
        /// <summary>
        /// Responds with a TSODBHouseBlob in a <c>TSOGetHouseBlobByIDResponse</c> packet.
        /// <para/>
        /// GZCLSID_cDBGetHouseBlobByID_Response
        /// </summary>
        GetHouseBlobByID_Response = 0xBBB8D0A7,
        /// <summary>
        /// Asks for Bookmarks. Data1 is the AvatarID
        /// </summary>
        GetBookmarks_Request = 0xFD8F9080,
        /// <summary>
        /// Responds with a list of Bookmarks. See: <c>TSOGetBookmarksResponse</c>
        /// </summary>
        GetBookmarks_Response = 0x3D8F9003,
        /// <summary>
        /// Logs the remote console
        /// </summary>
        InsertGenericLog_Request = 0x3D03D5F7,
        /// <summary>
        /// The payload of this packet is a new TSODBCharBlob to add to the server
        /// </summary>
        InsertNewCharBlob_Request = 0x9BB8EAC4,
        /// <summary>
        /// A response code. Known to cause issues with parsing if Size is too small.
        /// Perhaps its StatusCode plus a string message.
        /// Will raise a Avatar Creation Error message if the size is too small.
        /// </summary>
        InsertNewCharBlob_Response = 0x1BB8EB44,
        /// <summary>
        /// This is sent when loading into a NoNetworkHouse
        /// <code>GZCLSID_cDBUpdateLotValueByID_Request</code>        
        /// </summary>
        UpdateLotValueByID_Request = 0xDC17FB0E,
        /// <summary>
        /// This is sent when loading into a NoNetworkHouse
        /// <code>GZCLSID_cDBUpdateTaskStatus_Request</code>
        /// </summary>
        UpdateTaskStatus_Request = 0xA92AF562,
        /// <summary>
        /// This is sent when clicking the Top 100 List in the UI Gizmo
        /// </summary>
        GetTopList_Request = 0x3D8787DA,
        GetTopList_Response = 0xA928455B,
        /// <summary>
        /// This is sent when you click "Most Popular Places" in the UI Gizmo
        /// GZCLSID_cDBGetTopResultSetByID_Request
        /// </summary>
        GetTopResultSetByID_Request = 0xBCD038AC,
        /// <summary>
        /// This is sent after closing Edit A Sim and after the SetCharByID_Request PDU.
        /// <para/>This seems to set the money fields of the Avatar (Total Money, Passive Money, etc.)
        /// <para/> Seems very dangerous for the Client to be setting the money field -- especially since Money
        /// shouldn't change in CAS.
        /// </summary>
        SetMoneyFields_Request = 0x5CF147E8,
        /// <summary>
        /// Confirms with the Client what the money fields for the Avatar should be.
        /// </summary>
        SetMoneyFields_Response = 0xFCF14801,
        /// <summary>
        /// An exact-match search function for Sims and Houses
        /// </summary>
        SearchExactMatch_Request = 0xA952742D,
        /// <summary>
        /// The response packet to an exact-search
        /// </summary>
        SearchExactMatch_Response =  0x89527401,
        /// <summary>
        /// A general search, not exactly matching the resource requested
        /// </summary>
        Search_Request = 0x89483786,
        /// <summary>
        /// The response to a <see cref="Search_Request"/>
        /// </summary>
        Search_Response = 0xC94837CC,
        BuyLotByAvatarID_Request = 0x1D8DD55A,
        BuyLotByAvatarID_Response = 0xBD8DDB9B,
        DebitCredit_Request = 0x7C24F627,
        DebitCredit_Response = 0x3C24F6BC,
    }
    /// <summary>
    /// A kMSG is used to invoke a Regulator to change its state or respond to a stimulus.
    /// <para>For example, the DBServiceClientD will use the <see cref="kDBServiceRequestMsg"/> to send data.
    /// and uses the <see cref="kDBServiceResponseMsg"/> to be notified when to receive data.</para>
    /// </summary>
    public enum TSO_PreAlpha_kMSGs : uint
    {
        /// <summary>
        /// <see cref="TSODBRequestWrapper"/> PDUs that are intended as Requests
        /// </summary>
        kDBServiceRequestMsg  = 0x3BF82D4E,
        /// <summary>
        /// <see cref="TSODBRequestWrapper"/> PDUs that are intended as Responses
        /// </summary>
        kDBServiceResponseMsg = 0xDBF301A9
    }

    /// <summary>
    /// Search categories in TSO: Pre-Alpha sent from <see cref="TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request"/>
    /// or <see cref="TSO_PreAlpha_DBActionCLSIDs.Search_Request"/> PDUs
    /// </summary>
    public enum TSO_PreAlpha_SearchCategories : uint
    {
        None = 0x0,
        /// <summary>
        /// An Avatar search
        /// </summary>
        Avatar = 0x1,
        /// <summary>
        /// A House search
        /// </summary>
        House = 0x2,
    }

    /// <summary>
    /// Value types that are commonly found in PDUs from TSO: Pre-Alpha
    /// </summary>
    public enum TSOVoltronValueTypes
    {
        /// <summary>
        /// Default value.
        /// </summary>
        Invalid,
        /// <summary>
        /// A string that ends with a null-terminator
        /// </summary>
        NullTerminated,
        /// <summary>
        /// A length-prefixed string plus a <see cref="UInt16"/> indicating data type (0x8000)
        /// <para>Looks like: <c>{ 0x8000 [WORD LENGTH] [UTF-8 *LENGTH* byte array] }</c></para>
        /// </summary>
        Pascal,
        /// <summary>
        /// One-Byte length followed by the string in UTF-8 format
        /// <code>[byte Length][byte[] UTF-8]</code>
        /// </summary>
        Length_Prefixed_Byte,
        /// <summary>
        /// A little-endian numeric type
        /// </summary>
        LittleEndian,
        /// <summary>
        /// A big-endian numeric type
        /// </summary>
        BigEndian
    }
}

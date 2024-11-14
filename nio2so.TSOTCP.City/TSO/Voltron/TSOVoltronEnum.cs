namespace nio2so.TSOTCP.City.TSO.Voltron
{
    /// <summary>
    /// For discovering more PDUs, please refer to: http://niotso.org/files/prealpha_pdu_tables.txt
    /// </summary>
    public enum TSO_PreAlpha_VoltronPacketTypes : ushort
    {
        UNKNOWN_MSG_ID_PDU = 0x0,
        BYE_PDU = 0x06,
        CLIENT_ONLINE_PDU = 0x09,
        HOST_ONLINE_PDU = 0x1D,
        SET_ACCEPT_ALERTS_PDU = 0x31,
        SET_ACCEPT_ALERTS_RESPONSE = 0x32,
        SET_ACCEPT_ALERTS_RESPONSE_PDU = 0x0032,
        SET_IGNORE_LIST_PDU = 0x0033,
        SET_IGNORE_LIST_RESPONSE_PDU = 0x0034,
        SET_INVINCIBLE_PDU = 0x0035,
        SET_INVINCIBLE_RESPONSE_PDU = 0x0036,
        SET_INVISIBLE_PDU = 0x0037,
        SET_INVISIBLE_RESPONSE_PDU = 0x0038,
        SET_ROOM_NAME_PDU = 0x0039,
        SET_ROOM_NAME_RESPONSE_PDU = 0x003A,
        SET_ACCEPT_FLASHES_PDU = 0x0043,
        SET_ACCEPT_FLASHES_RESPONSE_PDU = 0x0044,
        DB_REQUEST_WRAPPER_PDU = 0x0082,
        BC_VERSION_LIST_PDU = 0x0092,
        EJECT_VISITOR_RESPONSE_PDU = 0x0095,
        LOAD_HOUSE_RESPONSE_PDU = 0x0099,
        UPDATE_PLAYER_PDU = 0x3C,
        HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU = 0x0097,
        HOST_OFFLINE_PDU = 0x001C,
        READ_PROFILE_PDU = 0x002D,
        READ_PROFILE_RESPONSE_PDU = 0x2E,

        SPLIT_BUFFER_PDU = 0x0045
    }
    /// <summary>
    /// <para>For discovering more CLSIDs, please refer to: http://niotso.org/files/prealpha_constants_table.txt</para>
    /// </summary>
    public enum TSO_PreAlpha_DBStructCLSIDs : uint
    {
        GZCLSID_cCrDMStandardMessage =      0x125194E5,
        cTSONetMessageStandard =            0x125194E5,
        GZCLSID_cCrDMTestObject =           0x122A94F2,
        cTSONetMessageStream =              0x125194F5,
        cTSOAvatarCreationRequest =         0x3EA44787,
        cTSOInterdictor =                   0xAA3ECCB3,
        cTSOInterdictionPass =              0xAA5FA4D8,
        cTSOInterdictionPassAndLog =        0xCA5FA4E0,
        cTSOInterdictionDrop =              0xCA5FA4E3,
        cTSOInterdictionDropAndLog =        0xCA5FA4EB,
        cTSONetMessageEnvelope =            0xAA7B191E,
        cTSOChannelMessageEnvelope =        0x2A7B4E6A,
        cTSODeadStream =                    0x0A9D7E3A,
        cTSOTopicUpdateMessage =            0x09736027,
        cTSODataTransportBuffer =           0x0A2C6585,
        cTSOTopicUpdateErrorMessage =       0x2A404946,
        GZPROBEID_cEAS =                    0x1D873D36
    }
    /// <summary>
    /// The Sims Online makes a distinction between Queries, Requests and Responses
    /// <para>Generally, the Query is what the game uses to invoke the DBAppService to make the Request packet.</para>
    /// <para>You can send the Query ID back to the client, but it appears to be ignored.</para>
    /// <para>When responding to a Request, you need to find the accompanying Response CLSID.</para>
    /// 
    /// <para>For discovering more CLSIDs, please refer to: http://niotso.org/files/prealpha_constants_table.txt</para>
    /// </summary>
    public enum TSO_PreAlpha_DBActionCLSIDs : uint
    {
        GetRoommateInfoByLotIDRequest = 0xFD3338E9,
        GetRoommateInfoByLotIDResponse = 0xDD3339EE,
        GetCharBlobByIDRequest = 0x5BB73FAB,
        GetCharBlobByIDResponse = 0x5BB73FE4,
        GetCharByIDRequest = 0x7BAE5079,
        GetRelationshipsByIDRequest = 0x3BF96A6C,
        GetLotListRequest = 0x5BEEB701,
        GetLotByIDRequest = 0xFBE96AA3,
        GetHouseLeaderByLotID = 0xDD909124,
        /// <summary>
        /// GZCLSID_cDBGetHouseBlobByID_Request
        /// </summary>
        GetHouseBlobByIDRequest = 0x5BB8D069,
        /// <summary>
        /// GZCLSID_cDBGetHouseBlobByID_Response
        /// </summary>
        GetHouseBlobByIDResponse = 0xBBB8D0A7,
        GetBookmarksRequest = 0xFD8F9080,
        GetBookmarksResponse = 0x3D8F9003,
        InsertGenericLog_Request = 0x3D03D5F7,
        InsertNewCharBlob_Request = 0x9BB8EAC4,
        InsertNewCharBlob_Response = 0x1BB8EB44,

    }

    public enum TSOVoltronValueTypes
    {
        Invalid,
        NullTerminated,
        Pascal,
        LittleEndian,
        BigEndian
    }
}

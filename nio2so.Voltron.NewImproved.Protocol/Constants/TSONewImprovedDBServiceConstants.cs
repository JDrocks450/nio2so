namespace nio2so.Voltron.NewImproved.Protocol.Constants
{
    public enum TSO_NewImproved_DBServiceConstants : uint
    {
        GetHouseThumbByID_Request = 0x9BF18F10,
        GetHouseThumbByID_Response = 0x9BF19573,
        GetHouseThumbByID_Message = 0x69A2E644,

        GetLotAndObjects_Request = 0x0BFD89E3,
        GetLotAndObjects_Response = 0x8C6B0A49,
        GetLotAndObjects_Message = 0x2BFB0268,

        GetLotList_Request = 0x5BEEB701,
        GetLotList_Response = 0xDBEECD65,
        GetLotList_Message = 0x69A2E641,

        GetMaxPlayerPerLot_Request = 0x5F0A0561,
        GetMaxPlayerPerLot_Response = 0xDF0A0528,
        GetMaxPlayerPerLot_Message = 0x3F0A07EA,

        GetNeighborhoods_Request = 0x8AE0FD8C,
        GetNeighborhoods_Response = 0x6AE0FD6E,
        GetNeighborhoods_Message = 0xAAE10F9A,

        GetShardVersion_Request = 0x5E209378,
        GetShardVersion_Response = 0x9E2095BE,
        GetShardVersion_Message = 0x9E209766,

        GetTopList_Request = 0x3D8787DA,
        GetTopList_Response = 0xA928455B,
        GetTopList_Message = 0xC9A2E6AD,

        GetTopResultSetByID_Request = 0xBCD038AC,
        GetTopResultSetByID_Response = 0xFCD03A1B,
        GetTopResultSetByID_Message = 0x69AC83C4,

        InsertBookmarks_Request = 0x09CBE333,
        InsertBookmarks_Response = 0x69CBE384,
        InsertBookmarks_Message = 0xE9CBE612,

        InsertGenericLog_Request = 0x3D03D5F7,
        InsertGenericLog_Message = 0xC9A2E6A1,

        InsertGenericTask_Request = 0xC98B6799,
        InsertGenericTask_Response = 0xA98B6783,
        InsertGenericTask_Message = 0x09A2E6DD,

        InsertNeighborhoods_Request = 0xAAE1247E,
        InsertNeighborhoods_Response = 0x4AE12821,
        InsertNeighborhoods_Message = 0x4AE12A15,

        InsertNewAvatar_Request = 0x8A3BE831,
        InsertNewAvatar_Response = 0x0A3C127F,
        InsertNewAvatar_Message = 0xAA3C1529,

        InsertNewFriendshipComment_Request = 0x6AE8E1ED,
        InsertNewFriendshipComment_Response = 0xEAE8E26F,
        InsertNewFriendshipComment_Message = 0x6AE9DC97,

        InsertPendingRoomateInv_Request = 0x3CE98067,
        InsertPendingRoomateInv_Response = 0xBCE989F6,
        InsertPendingRoomateInv_Message = 0xA9A2E68C,

        InsertSpotlightTextByLotID_Request = 0x8B8AE566,
        InsertSpotlightTextByLotID_Response = 0x0B8AE5E6,
        InsertSpotlightTextByLotID_Message = 0xAB9915A8,

        MoveOutByAvatarID_Request = 0x4CEEB62C,
        MoveOutByAvatarID_Response = 0x8CEEB761,
        MoveOutByAvatarID_Message = 0xECF2B8BE,

        LoadAvatarByID_Request = 0x2ADF7EED,
        LoadAvatarByID_Response = 0x2ADF8FF5,
        LoadAvatarByID_Message = 0x8ADF865D,

        MoveLotByID_Request = 0xEB42651A,
        MoveLotByID_Response = 0xAB4266CE,
        MoveLotByID_Message = 0xEB4271F3,

        PrtControlToggleByAvatarID_Request = 0x8A53F433,
        PrtControlToggleByAvatarID_Response = 0x6A53F3F6,
        PrtControlToggleByAvatarID_Message = 0x2A53F7CE,

        RejectPendingRoomateInv_Request = 0xDCE98959,
        RejectPendingRoomateInv_Message = 0xA9A2E68D,

        ReleaseAvatarLease_Request = 0x6B9EAECD,
        ReleaseAvatarLease_Response = 0xAB9EB010,
        ReleaseAvatarLease_Message = 0x0B9EAF64,

        RenewAvatarLease_Request = 0xCB9EAF2F,
        RenewAvatarLease_Response = 0x2B9EB082,
        RenewAvatarLease_Message = 0x0B9EAF6D,

        SaveAvatarByID_Request = 0x2ADDE378,
        SaveAvatarByID_Response = 0x2ADDE27E,
        SaveAvatarByID_Message = 0xEADDEA30,

        SaveLotAndObjectBlobByID_Request = 0xCBFD89A7,
        SaveLotAndObjectBlobByID_Response = 0xCBFD899C,
        SaveLotAndObjectBlobByID_Message = 0x2BFB0265,

        Search_Request = 0x89483786,
        Search_Response = 0xC94837CC,
        Search_Message = 0xE9A2E6C9,

        SearchExactMatch_Request = 0xA952742D,
        SearchExactMatch_Response = 0x89527401,
        SearchExactMatch_Message = 0xE9A2E6CC,

        SellObject_Request = 0x8BFD896A,
        SellObject_Response = 0x8BFD895F,
        SellObject_Message = 0x4BFB026E,

        SetFriendshipComment_Request = 0x0AE0AEB8,
        SetFriendshipComment_Response = 0xAAE0AE66,
        SetFriendshipComment_Message = 0x4AE0BDF2,

        SetHouseThumbByID_Request = 0xFBF6E364,
        SetHouseThumbByID_Message = 0x69A2E648,

        SetLotDesc_Request = 0x8A70B952,
        SetLotDesc_Response = 0x4A70B913,
        SetLotDesc_Message = 0xAA70C34C,

        SetLotHoursVisitedByID_Request = 0x7C02938C,
        SetLotHoursVisitedByID_Message = 0x89A2E660,

        SetLotName_Request = 0x6A70B931,
        SetLotName_Response = 0xAA70B969,
        SetLotName_Message = 0x4A70C304,

        SetMoneyFields_Request = 0x5CF147E8,
        SetMoneyFields_Response = 0xFCF14801,
        SetMoneyFields_Message = 0xC9A2E69D,

        StockDress_Request = 0x2B4510AC,
        StockDress_Response = 0x4B4510D5,
        StockDress_Message = 0x6B4516F1,

        UpdateBadgeByID_Request = 0xCAFB30AA,
        UpdateBadgeByID_Response = 0xCAFB31A4,
        UpdateBadgeByID_Message = 0xEAFB3AC1,

        UpdateCharDescByID_Request = 0xAA3FEDA1,
        UpdateCharDescByID_Response = 0x6A3FED66,
        UpdateCharDescByID_Message = 0xAA401B81,

        UpdateDataServiceLotAdmitInfo_AddAdmittedID_Request = 0xCA26E9CF,
        UpdateDataServiceLotAdmitInfo_AddAdmittedID_Response = 0xAA270399,
        UpdateDataServiceLotAdmitInfo_AddAdmittedID_Message = 0x6A270C4D,

        UpdateDataServiceLotAdmitInfo_AddBannedID_Request = 0xEA26E9F8,
        UpdateDataServiceLotAdmitInfo_AddBannedID_Response = 0xCA2703AF,
        UpdateDataServiceLotAdmitInfo_AddBannedID_Message = 0x6A270C52,

        UpdateDataServiceLotAdmitInfo_RemoveAdmittedID_Request = 0xEA26E9E4,
        UpdateDataServiceLotAdmitInfo_RemoveAdmittedID_Response = 0xCA2703C0,
        UpdateDataServiceLotAdmitInfo_RemoveAdmittedID_Message = 0x6A270C4F,

        UpdateDataServiceLotAdmitInfo_RemoveBannedID_Request = 0x0A26EA0C,
        UpdateDataServiceLotAdmitInfo_RemoveBannedID_Response = 0xEA2703DD,
        UpdateDataServiceLotAdmitInfo_RemoveBannedID_Message = 0x6A270C55,

        UpdateDataServiceLotAdmitInfo_SetAdmitMode_Request = 0xCA26E9BD,
        UpdateDataServiceLotAdmitInfo_SetAdmitMode_Response = 0x0A2703F4,
        UpdateDataServiceLotAdmitInfo_SetAdmitMode_Message = 0x6A270C49,

        UpdateLotValueByID_Request = 0xDC17FB0E,
        UpdateLotValueByID_Message = 0x89A2E668,

        UpdateTaskStatus_Request = 0xA92AF562,
        UpdateTaskStatus_Message = 0xE9A2E6C6,

        GetSpotlightLotList_Request = 0xEBD4DDAC,
        GetSpotlightLotList_Response = 0xCBD4DD94,
        GetSpotlightLotList_Message = 0xCBD4DD7E,

        GetFinancialDetail_Request = 0x0C23D673,
        GetFinancialDetail_Response = 0x4C23D6C1,
        GetFinancialDetail_Message = 0x2C23D7A1,

        GetOnlineJobLot_Request = 0x8C3BBA00,
        GetOnlineJobLot_Response = 0x2C3BC494,
        GetOnlineJobLot_Message = 0xAC3B863F,

        OnlineJobLotDesactivate_Request = 0xAC50944B,
        OnlineJobLotDesactivate_Response = 0x8C509438,
        OnlineJobLotDesactivate_Message = 0x8C509423,

        OnlineJobLotRequestDesactivation_Request = 0x0C9D233E,
        OnlineJobLotRequestDesactivation_Response = 0x4C9D236B,
        OnlineJobLotRequestDesactivation_Message = 0x4C9D247A,

        OnlineJobOccupantDesactivate_Request = 0xAC96E1AE,
        OnlineJobOccupantDesactivate_Response = 0xEC96E1E7,
        OnlineJobOccupantDesactivate_Message = 0xEC96E4FA,

        UpdatePrivacyModeByID_Request = 0xA0C6106C,
        UpdatePrivacyModeByID_Response = 0x20C61C89,
        UpdatePrivacyModeByID_Message = 0x20C7DD21,

        GetDataUpdateEventsLastSeqID_Request = 0xCBB127FD,
        GetDataUpdateEventsLastSeqID_Response = 0x0BB12839,
        GetDataUpdateEventsLastSeqID_Message = 0xABB128E4,

        GetDataUpdateEvents_Request = 0x0B5E2124,
        GetDataUpdateEvents_Response = 0x0B5E202B,
        GetDataUpdateEvents_Message = 0xCB5E26FC,

        GetNeighborhoodInfo_Request = 0xCB7AD7EE,
        GetNeighborhoodInfo_Response = 0x0B7ADA36,
        GetNeighborhoodInfo_Message = 0x0B7ADB35,

        CallCreateFriends_Request = 0x21586E78,
        CallCreateFriends_Response = 0x61586EB4,
        CallCreateFriends_Message = 0x21587C80,

        CallDecayRelationships_Request = 0xE1586F32,
        CallDecayRelationships_Response = 0xA1586F52,
        CallDecayRelationships_Message = 0xE1587C89,

        UpdateRelationshipLastContact_Request = 0x2D33ABF3,
        UpdateRelationshipLastContact_Response = 0xCD33BF75,
        UpdateRelationshipLastContact_Message = 0xCD33AB86,

        UpdatePreferedLanguageByID_Request = 0x2D98FAF3,
        UpdatePreferedLanguageByID_Message = 0xCD99038E,

        RenameAvatar_Request = 0x1060F3A1,
        RenameAvatar_Response = 0xB061033E,
        RenameAvatar_Message = 0x3060F7BC,

        AcceptPendingRoomateInv_Request = 0xDCE9801A,
        AcceptPendingRoomateInv_Response = 0xDCE98A01,
        AcceptPendingRoomateInv_Message = 0xA9A2E686,

        AcquireAvatarLease_Request = 0x6B9D76FC,
        AcquireAvatarLease_Response = 0x4B9D77CA,
        AcquireAvatarLease_Message = 0x2B9E5AE7,

        BuyDress_Request = 0x2B44F0C9,
        BuyDress_Response = 0xCB44F065,
        BuyDress_Message = 0xCB4255DF,

        BuyLotByAvatarID_Request = 0x1D8DD55A,
        BuyLotByAvatarID_Response = 0xBD8DDB9B,
        BuyLotByAvatarID_Message = 0xC9A2E6B3,

        BuyObject_Request = 0x4BFD8A1B,
        BuyObject_Response = 0x4BFD8A12,
        BuyObject_Message = 0x2BFB0269,

        DeleteAttributeByAvatarID_Request = 0xCB392F06,
        DeleteAttributeByAvatarID_Response = 0xCB393006,
        DeleteAttributeByAvatarID_Message = 0xEB43CB98,

        DeleteAvatarByID_Request = 0x6A52E10C,
        DeleteAvatarByID_Response = 0x4A52E0E6,
        DeleteAvatarByID_Message = 0x4A52F0D9,

        DeleteBookmarks_Request = 0x2A3C81DB,
        DeleteBookmarks_Response = 0x69CBE292,
        DeleteBookmarks_Message = 0xE9CBE601,

        DeleteObject_Request = 0x0B25CD36,
        DeleteObject_Response = 0x2B219B40,
        DeleteObject_Message = 0x2B21934E,

        DeleteSpotlightTextByLotID_Request = 0x6B8D6F5D,
        DeleteSpotlightTextByLotID_Response = 0x2B8D6F0B,
        DeleteSpotlightTextByLotID_Message = 0x8B991597,

        GetAvatarIDByName_Request = 0xA95BB3D7,
        GetAvatarIDByName_Response = 0xE95BB42F,
        GetAvatarIDByName_Message = 0xE9A2E6D0,

        GetAvatarInventoryByID_Request = 0x2BFD89FF,
        GetAvatarInventoryByID_Response = 0x2BFD89F5,
        GetAvatarInventoryByID_Message = 0x4BFB026F,

        GetBookmarks_Request = 0x0BA7F82C,
        GetBookmarks_Response = 0x3D8F9003,
        GetBookmarks_Message = 0xE9A2E6BF,

        GetCharByID_Request = 0x7BAE5079,
        GetCharByID_Response = 0x1BAE532A,
        GetCharByID_Message = 0x29A2E618,

        GetCityType_Request = 0x2A3927A2,
        GetCityType_Response = 0x4A3929BB,
        GetCityType_Message = 0xCA3959AF,

        GetDataServiceBuildableMapInfoByXY_Request = 0x4A3D75DA,
        GetDataServiceBuildableMapInfoByXY_Response = 0x6A3D7A09,
        GetDataServiceBuildableMapInfoByXY_Message = 0x8A3D7E26,

        GetDataServiceLotAdmitInfo_Request = 0x8A2456E6,
        GetDataServiceLotAdmitInfo_Response = 0xEA245646,
        GetDataServiceLotAdmitInfo_Message = 0x2A245F85,

        GetDataServiceLotInfoByXY_Request = 0xE9C81D19,
        GetDataServiceLotInfoByXY_Response = 0xE9C81D0A,
        GetDataServiceLotInfoByXY_Message = 0x09C82624,

        GetDebitCreditTask_Request = 0xE98A1700,
        GetDebitCreditTask_Response = 0x898A168C,
        GetDebitCreditTask_Message = 0x09A2E6DA,

        GetGenericFlash_Request = 0x9FE8B670,
        GetGenericFlash_Response = 0x3FE8B5A4,
        GetGenericFlash_Message = 0x5FF08AC5,

        GetHouseLeaderByLotID_Request = 0xDD909124,
        GetHouseLeaderByLotID_Response = 0xBD90911F,
        GetHouseLeaderByLotID_Message = 0xC9A2E6B6,

        GetObjectTuningVariables_Request = 0xEFAB815F,
        GetObjectTuningVariables_Response = 0xD6327954,
        GetObjectTuningVariables_Message = 0x245A2D07,

        GetObjectsAndCatalog_Request = 0x9525BE9F,
        GetObjectsAndCatalog_Response = 0x6E8EAC4E,
        GetObjectsAndCatalog_Message = 0x5F7CD545,

        CreateObject_Request = 0xABCD8986,
        CreateObject_Response = 0xABCD897C,
        CreateObject_Message = 0x3044F7BC,

        GetSelector_Request = 0x17C62EA8,
        GetSelector_Response = 0x2C371266,
        GetSelector_Message = 0x3045F7BC,

        GetGameConstants_Request = 0xCD999E31,
        GetGameConstants_Response = 0xE1299B7A,
        GetGameConstants_Message = 0x3046F7BC,

        RecordFeedEvent_Request = 0x688188BA,
        RecordFeedEvent_Response = 0x41C22C14,
        RecordFeedEvent_Message = 0xD293067E,

        DebitCredit_Request = 0x7C24F627,
        DebitCredit_Response = 0x3C24F6BC,
        DebitCredit_Message = 0x89A2E66C,

        GetAllRelationshipsByID_Request = 0xAB4A6098,
        GetAllRelationshipsByID_Response = 0xCB4A60AC,
        GetAllRelationshipsByID_Message = 0xEB4A55D0,

        GetDataServiceAvatarBudgetByID_Request = 0x4AA84714,
        GetDataServiceAvatarBudgetByID_Response = 0x4AA84819,
        GetDataServiceAvatarBudgetByID_Message = 0x0AA84DE5,

        GetDataServiceAvatarSkillsByID_Request = 0x0AA81112,
        GetDataServiceAvatarSkillsByID_Response = 0x8AA81196,
        GetDataServiceAvatarSkillsByID_Message = 0xAAA827A7,

        GetFriendshipWebCommentByID_Request = 0xEAE9F7E3,
        GetFriendshipWebCommentByID_Response = 0x0AE9F8F1,
        GetFriendshipWebCommentByID_Message = 0xAAEA216F,

        InsertGenericFlash_Request = 0xFFE8AC61,
        InsertGenericFlash_Response = 0x3FF83299,
        InsertGenericFlash_Message = 0x1FF08AB0,

        SecureTrade_Request = 0xABFD8986,
        SecureTrade_Response = 0xABFD897C,
        SecureTrade_Message = 0x4BFB0271,

        SetDataServiceAvatarVarSkillLockByID_Request = 0xCB9DE627,
        SetDataServiceAvatarVarSkillLockByID_Response = 0xAB9DE815,
        SetDataServiceAvatarVarSkillLockByID_Message = 0x0B9DE77E,

        SetLotCategory_Request = 0xAAD4C4D5,
        SetLotCategory_Response = 0xCAD4C4F8,
        SetLotCategory_Message = 0xEAD4F9DD,

        SetLotNeighborhoodID_Request = 0xEAF53BFB,
        SetLotNeighborhoodID_Response = 0x0AF53C18,
        SetLotNeighborhoodID_Message = 0x0AE8D6B3,

        SetNeighborhoodCenterGridXY_Request = 0x8B0847D4,
        SetNeighborhoodCenterGridXY_Response = 0xAB0848EF,
        SetNeighborhoodCenterGridXY_Message = 0x8B084FCE,

        InventoryTransfer_Request = 0x2BFDC6D0,
        InventoryTransfer_Response = 0x4BFDC6DB,
        InventoryTransfer_Message = 0x0BFDC794,

        GetSelectorApproval_Request = 0xA7F41927,
        GetSelectorApproval_Response = 0xA46A18AD,
        GetSelectorApproval_Message = 0xB411EEF5,

        DebitCreditSharedAccount_Request = 0x1C6A90C3,
        DebitCreditSharedAccount_Response = 0xBCD04D22,
        DebitCreditSharedAccount_Message = 0x3047F7BC,

        ExternalBankTransaction_Request = 0x51F76976,
        ExternalBankTransaction_Response = 0xF898CE34,
        ExternalBankTransaction_Message = 0x51F76976,
    }
}
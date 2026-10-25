using nio2so.DataService.Common.Types.Avatar;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;
using System.ComponentModel;
using System.Dynamic;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    /// <summary>
    /// Handles Data Service requests for transactions relating to Avatars
    /// </summary>
    internal class AvatarController : TSODataServiceControllerBase
    {
        public AvatarController() { }

        [TSODataServiceTypeHandler("Avatar", (uint)TSO_NewImproved_TSOData_Constants.Avatar)]
        public void Avatar_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            Context.Handled = true;
            return;

            // Old test code here ... using data service for this now

            //**download avatar data from nio2so
            if (!Sender.TryGetService<nio2soVoltronDataServiceClient>(out nio2soVoltronDataServiceClient? dataServiceClient) || dataServiceClient == null)
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));

            TSODBChar? avatarInfo = dataServiceClient.GetCharacterFileByAvatarID(new(Context.ObjectID.Value)).Result.Result;

            if (avatarInfo == null)
                throw new NullReferenceException(nameof(avatarInfo));

            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            obj.Avatar_Name = avatarInfo.AvatarName;
            obj.Avatar_Description = avatarInfo.AvatarDescription;

            obj.Avatar_Appearance.AvatarAppearance_BodyOutfitID = 0x000001600000000D;
            obj.Avatar_Appearance.AvatarAppearance_HeadOutfitID = 0x0000040F0000000D;
            obj.Avatar_Appearance.AvatarAppearance_SkinTone = 0x00;

            obj.Avatar_LotGridXY = avatarInfo.MyLotID;

            obj.Avatar_IsOnline = true;

            Context.Handled = true;
        }

        [TSODataServiceTypeHandler("MyAvatar", (uint)TSO_NewImproved_TSOData_Constants.MyAvatar)]
        public async void MyAvatar_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            var financialHistoryRec = new ExpandoObject();

            financialHistoryRec.TryAdd("FinancialHistoryRec_Amount", (int)10000);
            financialHistoryRec.TryAdd("FinancialHistoryRec_Date", "October 24th");

            var financialHistory = new ExpandoObject();

            financialHistory.TryAdd("FinancialHistory_TimeScale", (sbyte)0);
            financialHistory.TryAdd("FinancialHistory_Type", (sbyte)1);
            financialHistory.TryAdd("FinancialHistory_History", new object[] { financialHistoryRec });

            obj.Avatar_FinancialHistoryVec = new object[] { financialHistory };

            Context.Handled = true;
            return;
        }
    }
}

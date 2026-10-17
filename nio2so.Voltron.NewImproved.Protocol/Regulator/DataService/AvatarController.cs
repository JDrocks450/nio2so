using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{   
    /// <summary>
    /// Handles Data Service requests for transactions relating to Avatars
    /// </summary>
    internal class AvatarController : TSODataServiceControllerBase
    {
        public AvatarController() { }

        [TSODataServiceTypeHandler("Avatar", (uint)TSO_NewImproved_TSOData_StructIDs.Avatar)]
        public async void Avatar_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            //**download avatar data from nio2so
            if (!Sender.TryGetService<nio2soVoltronDataServiceClient>(out nio2soVoltronDataServiceClient? dataServiceClient) || dataServiceClient == null)
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));

            nio2so.DataService.Common.Types.Avatar.AvatarProfile? avatarInfo = (await dataServiceClient.GetAvatarProfileByAvatarID(new(Context.ObjectID.Value))).Result;

            if (avatarInfo == null)
                throw new NullReferenceException(nameof(avatarInfo));

            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            obj.Avatar_Name = avatarInfo.Name;
            obj.Avatar_Description = "Sample description.";

            obj.Avatar_Appearance.AvatarAppearance_BodyOutfitID = 0x000001600000000D;
            obj.Avatar_Appearance.AvatarAppearance_HeadOutfitID = 0x0000040F0000000D;
            obj.Avatar_Appearance.AvatarAppearance_SkinTone = 0x01;

            obj.Avatar_LotGridXY = 0x00B800D5;

            obj.Avatar_IsOnline = true;

            Context.Handled = true;
        }
    }
}

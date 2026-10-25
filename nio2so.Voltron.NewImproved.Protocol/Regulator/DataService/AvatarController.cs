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
            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            switch (Context.Direction)
            {
                case TSODataServiceHandlerDataDirection.FromDataService:
                    
                    break;
                case TSODataServiceHandlerDataDirection.FromTSOClient:

                    break;
            }
            
            obj.Avatar_IsOnline = obj.Avatar_Name == "Bisquick";

            Context.Handled = true;
            return;
        }

        [TSODataServiceTypeHandler("MyAvatar", (uint)TSO_NewImproved_TSOData_Constants.MyAvatar)]
        public async void MyAvatar_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            dynamic obj = Context.DataObject.GetObjectAsDynamic();
            Context.Handled = true;

            return;

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

using nio2so.Voltron.Core.Services;
using nio2so.Voltron.NewImproved.Protocol.Constants;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{    
    /// <summary>
    /// Handles Data Service requests for transactions relating to Avatars
    /// </summary>
    internal class AvatarController : TSODataServiceControllerBase
    {
        public AvatarController() { }

        [TSODataServiceTypeHandler("Bookmark", (uint)TSO_NewImproved_TSOData_Constants.Bookmark)]
        public void Bookmark_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            Context.Handled = true;
        }

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
            
            obj.Avatar_IsOnline = true;

            Context.Handled = true;
            return;
        }

        [TSODataServiceTypeHandler("MyAvatar", (uint)TSO_NewImproved_TSOData_Constants.MyAvatar)]
        public async void MyAvatar_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            dynamic obj = Context.DataObject.GetObjectAsDynamic();
            Context.Handled = true;

            //FinancialDetail
            {
                Sender.GetService<TSODataDefinitionService>().TryGetDataStruct((uint)TSO_NewImproved_TSOData_Constants.FinancialDetail, out var financialDetail);
                Sender.GetService<TSODataDefinitionService>().CreateObject(financialDetail, out var financialDetailObject);

                dynamic fd = financialDetailObject.GetObjectAsDynamic();
                fd.FinancialDetail_TimeFrame = (byte)0x01;
                fd.FinancialDetail_Type = (byte)0x01;

                Sender.GetService<TSODataDefinitionService>().TryGetDataStruct((uint)TSO_NewImproved_TSOData_Constants.FinancialDetailRec, out var financialDetailRec);
                Sender.GetService<TSODataDefinitionService>().CreateObject(financialDetailRec, out var financialDetailRecObject);

                dynamic rec = financialDetailRecObject.GetObjectAsDynamic();
                rec.FinancialDetailRec_Amount = (int)1000;
                rec.FinancialDetailRec_Category = (byte)0x01;

                fd.FinancialDetail_Detail = new object[] { financialDetailRecObject.Instance };

                obj.Avatar_FinancialDetailVec = new object[] { financialDetailObject.Instance };
            }
            //Fianncial History
            {
                Sender.GetService<TSODataDefinitionService>().TryGetDataStruct((uint)TSO_NewImproved_TSOData_Constants.FinancialHistory, out var financialHistory);
                Sender.GetService<TSODataDefinitionService>().CreateObject(financialHistory, out var financialHistoryObject);

                dynamic fh = financialHistoryObject.GetObjectAsDynamic();
                fh.FinancialHistory_TimeScale = (sbyte)0x03;
                fh.FinancialHistory_Type = (sbyte)0x01;

                Sender.GetService<TSODataDefinitionService>().TryGetDataStruct((uint)TSO_NewImproved_TSOData_Constants.FinancialDetailRec, out var financialHistoryRec);
                Sender.GetService<TSODataDefinitionService>().CreateObject(financialHistoryRec, out var financialHistoryRecObject);

                dynamic rec = financialHistoryRecObject.GetObjectAsDynamic();
                rec.FinancialHistoryRec_Amount = (int)2000;
                rec.FinancialHistoryRec_Date = "September 1st, 2008";

                fh.FinancialHistory_History = new object[] { financialHistoryRecObject.Instance };

                obj.Avatar_FinancialHistoryVec = new object[] { financialHistoryObject.Instance };
            }
            Context.Handled = true;
            return;
        }
    }
}

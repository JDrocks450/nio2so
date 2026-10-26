namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    /// <summary>
    /// Verifies data structs related to the Top100 Protocol
    /// </summary>
    internal class Top100Controller : TSODataServiceControllerBase
    {
        [TSODataServiceTypeHandler("Top100ListFilter", (uint)TSO_NewImproved_TSOData_Constants.Top100ListFilter)]
        public void Top100ListFilter_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            Context.Handled = true;

            dynamic obj = Context.DataObject.GetObjectAsDynamic();
            obj.Top100ListFilter_ResultsVec = new uint[1] { 12058837 };

            switch (Context.Direction)
            {
                case TSODataServiceHandlerDataDirection.FromDataService:

                    break;
                case TSODataServiceHandlerDataDirection.FromTSOClient:

                    break;
            }

            return;
        }
    }
}

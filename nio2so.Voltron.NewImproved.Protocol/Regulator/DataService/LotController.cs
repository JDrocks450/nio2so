namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    /// <summary>
    /// 
    /// </summary>
    internal class LotController : TSODataServiceControllerBase
    {
        public LotController() { }

        [TSODataServiceTypeHandler("Lot", (uint)TSO_NewImproved_TSOData_StructIDs.Lot)]
        public void Lot_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {           
            Context.Handled = true;
            
            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            obj.Lot_IsOnline = true;
        }
    }
}

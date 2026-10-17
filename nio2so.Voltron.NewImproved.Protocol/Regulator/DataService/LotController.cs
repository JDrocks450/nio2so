namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    /// <summary>
    /// 
    /// </summary>
    internal class LotController : TSODataServiceControllerBase
    {
        public LotController() { }

        [TSODataServiceTypeHandler("Lot", (uint)TSO_NewImproved_TSOData_StructIDs.Lot)]
        public void Lot_StructHandler(HandlerContext Context)
        {
            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            obj.Lot_Name = "Bisquick's Testing Zone";
            obj.Lot_Description = "You can't enter this lot yet because the server doesn't know how to do that.";
            obj.Lot_NeighborhoodName = "Admin Zone";

            obj.Lot_Price = 3000;

            obj.Lot_IsOnline = true;

            obj.Lot_Category = 0x30;

            Context.Handled = true;
        }
    }
}

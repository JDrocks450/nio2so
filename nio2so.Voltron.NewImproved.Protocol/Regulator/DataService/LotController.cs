using nio2so.Voltron.NewImproved.Protocol.Constants;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    /// <summary>
    /// 
    /// </summary>
    internal class LotController : TSODataServiceControllerBase
    {
        public LotController() { }

        [TSODataServiceTypeHandler("Lot", (uint)TSO_NewImproved_TSOData_Constants.Lot)]
        public void Lot_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {           
            Context.Handled = true;
            
            dynamic obj = Context.DataObject.GetObjectAsDynamic();
            if (Context.DataObject.GetObjectAsDictionary().ContainsKey("Lot_Thumbnail"))
            {
                obj.Lot_Thumbnail = GetThumbnail();
                
            }
            //obj.Lot_IsOnline = true;
        }

        public static byte[] GetThumbnail(string path = @"C:\nio2so\housethumbnails\city1\45351678.png")
        {
            return File.ReadAllBytes(path);
        }
    }
}

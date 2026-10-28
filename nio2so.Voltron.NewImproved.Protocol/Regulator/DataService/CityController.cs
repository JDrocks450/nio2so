using nio2so.Voltron.NewImproved.Protocol.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Voltron.NewImproved.Protocol.Regulator.DataService.TSODataServiceControllerBase;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    internal class CityController
    {
        [TSODataServiceTypeHandler("City", (uint)TSO_NewImproved_TSOData_Constants.City)]
        public void City_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            Context.Handled = true;
            return;
        }
        [TSODataServiceTypeHandler("CurrentCity")]
        public void CurrentCity_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            dynamic obj = Context.DataObject.GetObjectAsDynamic();

            var dict = new Dictionary<uint, object>();
            obj.City_ReservedLotInfo = dict;

            dict.Add(12058837, true);

            Context.Handled = true;
            return;
        }

        [TSODataServiceTypeHandler("Neighborhood", (uint)TSO_NewImproved_TSOData_Constants.Neighborhood)]
        public void Neighborhood_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            dynamic value = Context.DataObject.GetObjectAsDynamic();

            value.Neighborhood_LotCount = value.Neighborhood_LotIDVec.Length;

            Context.Handled = true;
            return;
        }
        [TSODataServiceTypeHandler("MapView_NearZoom_Neighborhood", (uint)TSO_NewImproved_TSOData_Constants.MapView_NearZoom_Neighborhood)]
        public void MapView_NearZoom_Neighborhood_StructHandler(TSOProtocol Sender, HandlerContext Context)
        {
            dynamic value = Context.DataObject.GetObjectAsDynamic();

            //value.Neighborhood_LotCount = value.Neighborhood_LotIDVec.Length;

            Context.Handled = true;
            return;
        }
    }
}

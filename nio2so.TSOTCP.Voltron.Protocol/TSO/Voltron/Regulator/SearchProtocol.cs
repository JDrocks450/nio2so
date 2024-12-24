using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator(nameof(SearchProtocol))]
    internal class SearchProtocol : TSOProtocol
    {
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request)]
        public void SearchExactMatch_Request(TSODBRequestWrapper PDU)
        { // Exact search
            TSOExactSearchRequest searchPDU = (TSOExactSearchRequest)PDU;
            string searchTerm = searchPDU.SearchTerm;
            TSO_PreAlpha_SearchCategories category = searchPDU.SearchResourceType;

            RespondTo(PDU, new TSOExactSearchResponse(category, TSOVoltronConst.MyAvatarID));
        }
    }
}

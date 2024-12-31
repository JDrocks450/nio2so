using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
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

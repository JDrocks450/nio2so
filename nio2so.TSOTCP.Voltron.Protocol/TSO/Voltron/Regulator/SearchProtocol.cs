using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator(nameof(SearchProtocol))]
    internal class SearchProtocol : TSOProtocol
    {
        /// <summary>
        /// Handles an incoming <see cref="TSOSearchRequest"/> PDU request
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request)]
        public void SearchExactMatch_Request(TSODBRequestWrapper PDU)
        { // Exact search
            UnifiedSearchHandler((TSOSearchRequest)PDU);
        }

        /// <summary>
        /// Handles an incoming <see cref="TSOSearchRequest"/> PDU request
        /// </summary>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.Search_Request)]
        public void SearchWildcard_Request(TSODBRequestWrapper PDU)
        { // Broadly search
            UnifiedSearchHandler((TSOSearchRequest)PDU);
        }

        private void UnifiedSearchHandler(TSOSearchRequest SearchPDU)
        {
            //determine if we're exactly searching or broadly searching based on incoming data from the remote connection
            bool IsExactMatch = SearchPDU is TSOExactSearchRequest;
            TSOSearchRequest searchPDU = SearchPDU;
            string searchTerm = searchPDU.SearchQuery;
            TSO_PreAlpha_Categories category = searchPDU.SearchCategory;
            //SEARCH
            TSOSearchResultStruct[] results = DoSearch(IsExactMatch, searchTerm, category).ToArray();
            RespondWith(IsExactMatch ? new TSOExactSearchResponse(searchTerm, category, results) : new TSOSearchResponse(searchTerm, category, results));
        }

        /// <summary>
        /// Represents a test search feature supporting an avatar result and a house result
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public IEnumerable<TSOSearchResultStruct> DoSearch(bool IsExactMatch, string searchTerm, TSO_PreAlpha_Categories category)
        { // Exact search
            switch (category)
            {
                case TSO_PreAlpha_Categories.Avatar:
                    yield return new TSOSearchResultStruct(TestingConstraints.MyFriendAvatarID, TestingConstraints.MyFriendAvatarName);
                    break;
                case TSO_PreAlpha_Categories.House:
                    yield return new TSOSearchResultStruct(TestingConstraints.MyHouseID, TestingConstraints.MyHouseName);
                    break;
            }
        }
    }
}

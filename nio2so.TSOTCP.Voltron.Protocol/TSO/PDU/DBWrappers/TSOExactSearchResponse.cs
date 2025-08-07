using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response)]
    public class TSOExactSearchResponse : TSODBRequestWrapper
    {        
        [TSOVoltronDBWrapperField] public TSOSearchResponseStruct SearchResponse { get; set; }
        
        public TSOExactSearchResponse() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response
            ) { }        

        public TSOExactSearchResponse(string SearchTerm, TSO_PreAlpha_Categories SearchType, params TSOSearchResultStruct[] Results) : this()
        {
            SearchResponse = new(SearchTerm,SearchType,Results);
            MakeBodyFromProperties();
        }
    }

    /// <summary>
    /// Responds to a <see cref="TSOSearchRequest"/> with a list of <see cref="TSOSearchResultStruct"/> items
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.Search_Response)]
    public class TSOSearchResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// <inheritdoc cref="TSOSearchResultStruct"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public TSOSearchResponseStruct SearchResponse { get; set; }
        /// <summary>
        /// Creates a new <see cref="TSOSearchResponse"/>
        /// </summary>
        public TSOSearchResponse() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.Search_Response
            )
        { }
        /// <summary>
        /// <inheritdoc cref="TSOSearchResponse()"/> with the given results and parameters
        /// </summary>
        /// <param name="SearchTerm">What the player searches for</param>
        /// <param name="SearchType">The type of item we're searching for</param>
        /// <param name="Results">The results from the player's search query</param>
        public TSOSearchResponse(string SearchTerm, TSO_PreAlpha_Categories SearchType, params TSOSearchResultStruct[] Results) : this()
        {
            SearchResponse = new(SearchTerm, SearchType, Results);
            MakeBodyFromProperties();
        }
    }
}

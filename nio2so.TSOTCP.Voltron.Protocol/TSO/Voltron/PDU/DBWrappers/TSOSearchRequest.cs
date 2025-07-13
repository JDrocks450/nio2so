using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when a Client is requesting Search Results with a given <see cref="SearchQuery"/> and <see cref="SearchCategory"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.Search_Request)]
    public class TSOSearchRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// What the player typed into the search box -- what they're searching for
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string SearchQuery { get; set; } = "";
        /// <summary>
        /// The category to search for a resource in
        /// </summary>
        [TSOVoltronDBWrapperField] public TSO_PreAlpha_Categories SearchCategory { get; set; } = TSO_PreAlpha_Categories.Avatar;
        /// <summary>
        /// Unknown
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Parameter3 { get; set; } = 0x0;
        
        /// <summary>
        /// Creates a new <see cref="TSOSearchRequest"/>
        /// </summary>
        public TSOSearchRequest() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
                TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request
            ) 
        {
            
        }
        /// <summary>
        /// <inheritdoc cref="TSOSearchRequest()"/> with specified parameters
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="searchCategory"></param>
        /// <param name="parameter3"></param>
        public TSOSearchRequest(string searchTerm, TSO_PreAlpha_Categories searchCategory, uint parameter3 = 0x0) : this()
        {
            SearchQuery = searchTerm;
            SearchCategory = searchCategory;
            Parameter3 = parameter3;
            MakeBodyFromProperties();
        }
    }
}

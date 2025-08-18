using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// Sent when a Client is requesting Exact Search Results with a given <see cref="SearchQuery"/> and <see cref="SearchCategory"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request)]
    public class TSOExactSearchRequest : TSOSearchRequest
    {
        /// <summary>
        /// <inheritdoc cref="TSOSearchRequest.SearchQuery"/>
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public new string SearchQuery { get => base.SearchQuery; set => base.SearchQuery = value; }
        /// <summary>
        /// <inheritdoc cref="TSOSearchRequest.SearchCategory"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public new TSO_PreAlpha_Categories SearchCategory { get => base.SearchCategory; set => base.SearchCategory = value; }
        /// <summary>
        /// <inheritdoc cref="TSOSearchRequest.Parameter3"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public new uint Parameter3 { get => base.Parameter3; set => base.Parameter3 = value; }

        /// <summary>
        /// <inheritdoc cref="TSOSearchRequest()"/>
        /// </summary>
        public TSOExactSearchRequest() : base()
        {

        }
        /// <summary>
        /// <inheritdoc cref="TSOSearchRequest(string,TSO_PreAlpha_Categories,uint)"/>
        /// </summary>
        public TSOExactSearchRequest(string searchTerm, TSO_PreAlpha_Categories searchCategory, uint parameter3 = 0x0) : base(searchTerm, searchCategory, parameter3) { }
    }
}

using nio2so.Data.Common.Serialization.Voltron;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request)]
    public class TSOExactSearchRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// What the player typed into the search box -- what they're searching for
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string SearchTerm { get; set; }
        /// <summary>
        /// The category to search for a resource in
        /// </summary>
        [TSOVoltronDBWrapperField] public uint SearchCategory { get; set; }
        /// <summary>
        /// Friendlier way to view the <see cref="SearchCategory"/>
        /// <para/> This property is omitted from the PDU payload, it is <see cref="TSOVoltronIgnorable"/>
        /// </summary>
        [TSOVoltronIgnorable][IgnoreDataMember] public TSO_PreAlpha_Categories SearchResourceType => (TSO_PreAlpha_Categories)SearchCategory;
        /// <summary>
        /// Unknown
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Parameter3 { get; set; }

        public TSOExactSearchRequest() : base() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request)]
    internal class TSOExactSearchRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// What the player typed into the search box -- what they're searching for
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string SearchTerm { get; set; }
        /// <summary>
        /// The category to search for a resource in
        /// </summary>
        [TSOVoltronDBWrapperField] public uint SearchCategory { get; set; }
        /// <summary>
        /// Friendlier way to view the <see cref="SearchCategory"/>
        /// <para/> This property is omitted from the PDU payload, it is <see cref="TSOVoltronIgnorable"/>
        /// </summary>
        [TSOVoltronIgnorable] public TSO_PreAlpha_SearchCategories SearchResourceType => (TSO_PreAlpha_SearchCategories)SearchCategory;
        /// <summary>
        /// Unknown
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Parameter3 { get; set; }

        public TSOExactSearchRequest() : base() { }
    }
}

using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Data.Common.Testing;
using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Util;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response)]
    public class TSOExactSearchResponse : TSODBRequestWrapper
    {
        public record TSOSearchResultStruct(uint ItemID, [property: TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] string ItemName);

        /// <summary>
        /// What the player typed into the search box -- what they're searching for
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string SearchTerm { get; set; }
        /// <summary>
        /// The category to search for a resource in
        /// </summary>
        [TSOVoltronDBWrapperField] public uint SearchCategory { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        /// <summary>
        /// List of returned search results
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronArrayLength(nameof(Results))] public uint ResultsCount { get; set; }
        [TSOVoltronDBWrapperField] public TSOSearchResultStruct[] Results { get; set; } = new TSOSearchResultStruct[0];
        [TSOVoltronDBWrapperField] public byte[] ReservedBytes { get; set; } = TSOVoltronArrayFillFunction.TSOFillArray(Array.Empty<byte>(), 32);
        [TSOVoltronDBWrapperField] public uint Reserved { get; set; } = 0x0;

        /// <summary>
        /// Friendlier way to view the <see cref="SearchCategory"/>
        /// <para/> This property is omitted from the PDU payload, it is <see cref="TSOVoltronIgnorable"/>
        /// </summary>
        [TSOVoltronIgnorable][IgnoreDataMember] public TSO_PreAlpha_Categories SearchResourceType => (TSO_PreAlpha_Categories)SearchCategory;

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOExactSearchResponse() : base() { }

        public TSOExactSearchResponse(string SearchTerm, TSO_PreAlpha_Categories SearchType, params TSOSearchResultStruct[] Results) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response
            )
        {
            this.SearchTerm = SearchTerm;
            SearchCategory = (uint)SearchType;
            this.Results = Results;

            MakeBodyFromProperties();
        }
    }
}

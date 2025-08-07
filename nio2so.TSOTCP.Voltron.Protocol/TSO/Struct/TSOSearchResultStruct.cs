using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Util;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Struct
{
    /// <summary>
    /// Represents a search result in the <see cref="TSOSearchResponseStruct"/>
    /// </summary>
    /// <param name="ItemID"></param>
    /// <param name="ItemName"></param>
    [DataContract]
    public record TSOSearchResultStruct(uint ItemID, [property: TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] string ItemName);

    /// <summary>
    /// The response structure for a <see cref="TSOSearchResponse"/> encompassing the <see cref="SearchQuery"/>, <see cref="SearchCategory"/> and <see cref="Results"/>
    /// </summary>
    [DataContract]
    public record TSOSearchResponseStruct()
    {
        /// <summary>
        /// What the player typed into the search box -- their inputted search term
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string SearchQuery { get; set; } = "";
        /// <summary>
        /// The category to search for a resource in
        /// </summary>
        public TSO_PreAlpha_Categories SearchCategory { get; set; } = TSO_PreAlpha_Categories.None;
        public uint Arg1 { get; set; }
        /// <summary>
        /// <inheritdoc cref="TSOVoltronArrayLength"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(Results))] public uint ResultsCount { get; set; }
        /// <summary>
        /// List of returned <see cref="TSOSearchResultStruct"/> items
        /// </summary>
        public TSOSearchResultStruct[] Results { get; set; } = new TSOSearchResultStruct[0];
        public byte[] ReservedBytes { get; set; } = Array.Empty<byte>().TSOFillArray(32);
        public uint Reserved { get; set; } = 0x0;

        /// <summary>
        /// Creates a new <see cref="TSOSearchResponseStruct"/> with the given parameters
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <param name="SearchType"></param>
        /// <param name="Results"></param>
        public TSOSearchResponseStruct(string SearchTerm, TSO_PreAlpha_Categories SearchType, params TSOSearchResultStruct[] Results) : this()
        {
            SearchQuery = SearchTerm;
            SearchCategory = SearchType;
            this.Results = Results;
        }
    }
}

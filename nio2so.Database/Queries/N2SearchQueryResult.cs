using static nio2so.DataService.Common.Queries.N2SearchQueryResult;

namespace nio2so.DataService.Common.Queries
{
    /// <summary>
    /// Result of a <see cref="SearchController"/> query
    /// </summary>
    /// <param name="SearchQuery"></param>
    /// <param name="SearchType"></param>
    /// <param name="ResultIDs"></param>
    public record N2SearchQueryResult(string SearchQuery, string SearchType, IEnumerable<SearchResultItem> ResultIDs)
    {
        public record SearchResultItem(uint ID, string Name);
    }
}

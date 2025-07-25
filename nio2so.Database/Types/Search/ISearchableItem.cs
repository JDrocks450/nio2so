namespace nio2so.DataService.Common.Types.Search
{
    public interface ISearchableItem
    {
        /// <summary>
        /// Words that can be matched with the search query to determine a match.
        /// </summary>
        IEnumerable<string> SearchableKeywords { get; }
    }
}

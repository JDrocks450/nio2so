namespace nio2so.DataService.Common.Types.Search
{
    public interface ISearchable<TID>
    {
        /// <summary>
        /// An exact search function that will match the <paramref name="QueryString"/> to a file in this data service object
        /// </summary>
        /// <param name="QueryString"></param>
        /// <returns></returns>
        IDictionary<TID,string> SearchExact(string QueryString);
        /// <summary>
        /// A search function that will loosely match the <paramref name="QueryString"/> to a file in this data service object
        /// </summary>
        /// <param name="QueryString"></param>
        /// <returns></returns>
        IDictionary<TID,string> SearchBroad(string QueryString, int MaxResults = 50);
    }
}

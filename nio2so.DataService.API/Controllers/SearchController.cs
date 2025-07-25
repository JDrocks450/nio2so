using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Types.Search;

namespace nio2so.DataService.API.Controllers
{   
    /// <summary>
    /// Handles incoming search requests
    /// </summary>
    [ApiController]
    [Route("api/search")]
    public class SearchController : DataServiceControllerBase
    {
        private readonly ILogger<SearchController> logger;

        public SearchController(ILogger<SearchController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("{Type}")]
        public ActionResult<N2SearchQueryResult> Search(string Type, [FromQuery] string Query)
        {
            IDictionary<uint, string> ids = new Dictionary<uint, string>();
            switch (Type.ToLowerInvariant().Trim())
            {
                case "avatar":
                    ids = (((ISearchable<uint>)APIDataServices.AvatarDataService).SearchBroad(Query));
                    break;
                case "house":
                    ids = (((ISearchable<uint>)APIDataServices.LotDataService).SearchBroad(Query));
                    break;
            }
            return new N2SearchQueryResult(Query, Type, ids.Select(x => new N2SearchQueryResult.SearchResultItem(x.Key, x.Value)));
        }

        [HttpGet("{Type}/exact")]
        public ActionResult<N2SearchQueryResult> SearchExact(string Type, [FromQuery] string Query)
        {
            IDictionary<uint, string> ids = new Dictionary<uint,string>();
            switch (Type.ToLowerInvariant().Trim())
            {
                case "avatar":
                    ids = (((ISearchable<uint>)APIDataServices.AvatarDataService).SearchExact(Query));
                    break;
                case "house":
                    ids = (((ISearchable<uint>)APIDataServices.LotDataService).SearchExact(Query));
                    break;
            }
            return new N2SearchQueryResult(Query, Type, ids.Select(x => new N2SearchQueryResult.SearchResultItem(x.Key,x.Value)));
        }
    }
}

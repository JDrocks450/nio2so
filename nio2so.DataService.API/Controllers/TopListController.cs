using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Types.Top100;

namespace nio2so.DataService.API.Controllers
{
    [Route("api/top100")]
    [ApiController]
    public class TopListController : DataServiceControllerBase
    {
        private ILogger<TopListController> logger;

        Top100DataService dataService => APIDataServices.Top100Service;

        public TopListController(ILogger<TopListController> Logger) : base()
        {
            logger = Logger;
        }

        // GET api/top100
        [HttpGet]
        public ActionResult<IEnumerable<Top100ListInfo>> GetTop100ListsAsync() => Ok(dataService.GetTop100Lists().Result);            
    }
}

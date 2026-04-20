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

        /// <summary>
        /// Returns all Top 100 Lists currently configured
        /// <code> GET api/top100</code>
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Top100ListInfo>>> GetTop100ListsAsync() => Ok(await dataService.GetTop100Lists());
        /// <summary>
        /// Returns all items in a Top 100 List by its ListID property
        /// <code> GET api/top100/[ListID]</code>
        /// </summary>
        /// <returns></returns>        
        [HttpGet("{ListID}")]
        public ActionResult<Top100ListItemsInfo> GetTop100ListsAsync(int ListID)
        {
            Top100ListItemsInfo? data = dataService.GetItemsByListID((uint)ListID);
            if (data == null) return NotFound();
            return Ok(data);
        }
    }
}

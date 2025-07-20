using Microsoft.AspNetCore.Mvc;
using nio2so.Database.Types;
using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserDataController : ControllerBase
    {
        private readonly ILogger<UserDataController> _logger;

        public UserDataController(ILogger<UserDataController> logger)
        {
            _logger = logger;
        }

        // GET: api/<AccountsController>
        [HttpGet]
        public ActionResult Get()
        {
            return BadRequest("Please provide a UserToken.");
        }

        // GET api/<AccountsController>/1337
        [HttpGet("{userID}")]
        public ActionResult<uint> Get(uint userID)
        {
            UserInfo? info = null;
            try
            {
                info = APIDataServices.UserDataService.GetUserInfoByUserToken(userID);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            if (info == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Requested resource could not be found.");
            return Ok(info);
        }
    }
}

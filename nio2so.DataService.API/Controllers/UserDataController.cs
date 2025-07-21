using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;

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

        // GET: api/users
        [HttpGet]
        public ActionResult Get()
        {
            return BadRequest("Please provide a UserToken.");
        }

        // GET api/<AccountsController>/bloaty@1
        [HttpGet("{UserTokenString}")]
        public ActionResult<UserInfo> Get(string UserTokenString)
        {
            UserToken token = UserTokenString;
            UserInfo? info = null;
            try
            {
                info = APIDataServices.UserDataService.GetUserInfoByUserToken(token);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            if (info == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Requested resource could not be found.");
            return info;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using System.Net;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountsController : DataServiceControllerBase
    {
        private readonly ILogger<AccountsController> logger;

        private AccountsDataService service => APIDataServices.AccountService;

        public AccountsController(ILogger<AccountsController> Logger) : base()
        {
            logger = Logger;
        }

        bool CreateAccount(string username)
        {
            var newToken = service.CreateAccount(username);
            return APIDataServices.UserDataService.CreateUserInfoFile(newToken, out _);
        }

        // GET: api/<AccountsController>
        [HttpGet]
        public ActionResult Get()
        {
            return BadRequest("Please provide a username.");
        }

        // GET api/<AccountsController>/bloaty#0001
        [HttpGet("{username}")]
        public ActionResult<N2AccountByUserNameQueryResult> Get(string username)
        {
            UserToken? token = null;
            try
            {
                if (!service.AccountExists(username))
                    if (!CreateAccount(username))
                        throw new InvalidOperationException("Could not create account/userfile for " + username);
                token = service.GetUserTokenByUserName(username);
            }
            catch (Exception ex)
            {               
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            if (token == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Requested resource could not be found.");
            return new N2AccountByUserNameQueryResult(username, token.Value);
        }

        // POST api/<AccountsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            throw new NotImplementedException("update");
        }

        // PUT api/<AccountsController>/bloaty
        [HttpPut("{UserName}")]
        public ActionResult Put(string UserName)
        { // **create a new account**
            service.CreateAccount(UserName);
            return Created();
        }

        // DELETE api/<AccountsController>/bloaty, <code e.g. 123456>
        [HttpDelete("{id}")]
        public void Delete(string UserName, string ChallengeCode)
        { 
            // will use a challenge code acquired by the server using some sort of confirmation dialog the user will type in to confirm account deletion
            // not implemented
            throw new NotImplementedException("Delete");
        }
    }
}

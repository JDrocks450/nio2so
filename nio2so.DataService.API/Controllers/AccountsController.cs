using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.Common.Tokens;
using System.Net;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        // GET: api/<AccountsController>
        [HttpGet]
        public ActionResult Get()
        {
            return BadRequest("Please provide a username.");
        }

        // GET api/<AccountsController>/1337
        [HttpGet("{username}")]
        public ActionResult<uint> Get(string username)
        {
            UserToken? token = null;
            try
            {
                token = APIDataServices.AccountService.GetUserTokenByUserName(username);
            }
            catch (Exception ex)
            {                
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            if (token == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Requested resource could not be found.");
            return Ok(token.Value.TokenValue);
        }

        // POST api/<AccountsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            throw new NotImplementedException("update");
        }

        // PUT api/<AccountsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            throw new NotImplementedException("create");
        }

        // DELETE api/<AccountsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotImplementedException("Delete");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Types.Avatar;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/avatars")]
    [ApiController]
    public class AvatarDataController : ControllerBase
    {
        private static AvatarDataService avatarDataService => APIDataServices.AvatarDataService;

        // GET: api/<AvatarDataController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/avatars/bloaty@1/1337
        [HttpGet("{AvatarID}/profile")]
        public ActionResult<AvatarProfile> GetAvatarProfile(uint AvatarID)
        {
            AvatarProfile? profile = null;
            try
            {
                profile = avatarDataService.GetProfileByAvatarID(AvatarID);
            }
            catch(KeyNotFoundException ke)
            {
                return NotFound(AvatarID);
            }
            if (profile == null) 
                return NotFound(AvatarID);
            return profile;
        }

        // POST api/<AvatarDataController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AvatarDataController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AvatarDataController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

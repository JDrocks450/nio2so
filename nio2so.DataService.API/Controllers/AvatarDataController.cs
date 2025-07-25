using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/avatars")]
    [ApiController]
    public class AvatarDataController : DataServiceControllerBase
    {
        private ILogger<AvatarDataController> logger;

        private static AvatarDataService avatarDataService => APIDataServices.AvatarDataService;

        public AvatarDataController(ILogger<AvatarDataController> Logger) : base()
        {
            logger = Logger;
        }

        // GET api/avatars/1337/profile
        [HttpGet("{AvatarID}/profile")]
        public ActionResult<AvatarProfile> GetAvatarProfile(uint AvatarID) => 
            GetObjectByID(avatarDataService.GetProfileByAvatarID, (AvatarIDToken)AvatarID);
        // GET api/avatars/1337/name
        [HttpGet("{AvatarID}/name")]
        public ActionResult<string> GetAvatarNameByID(uint AvatarID) =>
            GetObjectByID(avatarDataService.GetNameByID, (AvatarIDToken)AvatarID);

        // GET api/avatars/1337/bookmarks?searchType=avatar
        [HttpGet("{AvatarID}/bookmarks")]
        public ActionResult<N2BookmarksByAvatarIDQueryResult> GetAvatarBookmarks(uint AvatarID, [FromQuery] string listType)
        {
            ActionResult<AvatarInfo.AvatarBookmarkInfo> result = GetObjectByID(avatarDataService.GetBookmarksByAvatarID, (AvatarIDToken)AvatarID);
            if (result.Result is not null) return result.Result;
            AvatarInfo.AvatarBookmarkInfo? bookmarkInfo = result.Value as AvatarInfo.AvatarBookmarkInfo;
            if (bookmarkInfo == default) return NotFound(AvatarID);
            //--switch search type
            switch (listType)
            {
                case "avatar":
                    return new N2BookmarksByAvatarIDQueryResult(AvatarID, bookmarkInfo.BookmarkAvatars);
                default: return BadRequest();
            }
        }
        // POST api/avatars/1337/bookmarks?searchType=avatar
        [HttpPost("{AvatarID}/bookmarks")]
        public ActionResult SetAvatarBookmarks(uint AvatarID, [FromQuery] string listType, [FromBody] N2BookmarksByAvatarIDQueryResult Data)
        {
            if (avatarDataService.SetBookmarksByAvatarID(Data.AvatarID, Data.Avatars))
                return Ok();
            return BadRequest();
        }

        // GET api/avatars/1337/char
        [HttpGet("{AvatarID}/char")]
        public ActionResult<TSODBChar> GetCharacterFileByAvatarID(uint AvatarID) => 
            GetObjectByID(avatarDataService.GetCharacterByAvatarID, (AvatarIDToken)AvatarID);

        // POST api/avatars/1337/char
        [HttpPost("{AvatarID}/char")]
        public ActionResult SetCharacterFileByAvatarID(uint AvatarID, [FromBody] TSODBChar CharacterFile)
        {
            if (avatarDataService.SetCharacterByAvatarID(AvatarID, CharacterFile))
                return Ok();
            return BadRequest();
        }

        /// <summary>
        /// Creates a new Avatar, returns the <see cref="AvatarIDToken"/> of the newly created avatar, and takes in a method string describing what application is used to create the avatar
        /// </summary>
        /// <param name="User"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        // GET api/avatars/create
        [HttpGet("create")]
        public ActionResult<uint> CreateNewAvatar([FromQuery] string user, [FromQuery] string method) => (uint)avatarDataService.CreateNewAvatar((UserToken)user, method);

        // GET api/avatars/1337/appearance
        [HttpGet("{AvatarID}/appearance")]
        public async Task GetAvatarCharBlobAsync(uint AvatarID) => await MIMEResponse((await GetObjectByIDAsync(avatarDataService.GetCharBlobByAvatarID, (AvatarIDToken)AvatarID)).Value as byte[]);

        // POST api/avatars/1337/appearance
        [HttpPost("{AvatarID}/appearance")]
        public async Task<ActionResult> SetAvatarCharBlobAsync(uint AvatarID)
        {
            if (Request.ContentType != "application/octet-stream")
                return StatusCode(StatusCodes.Status406NotAcceptable);
            byte[] Data = new byte[(int)Request.ContentLength];
            await Request.Body.ReadExactlyAsync(Data);
            await avatarDataService.SetCharBlobByAvatarID(AvatarID, Data);
            return Ok();
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

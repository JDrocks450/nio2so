using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/avatars")]
    [ApiController]
    public class AvatarDataController : DataServiceControllerBase
    {
        private ILogger<AvatarDataController> logger;

        private static AvatarDataService avatarDataService => GetUnderlyingDataService<AvatarDataService>();

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
        public async Task<ActionResult> SetAvatarBookmarks(uint AvatarID, [FromQuery] string listType, [FromBody] N2BookmarksByAvatarIDQueryResult Data)
        {
            if (await avatarDataService.SetBookmarksByAvatarID(Data.AvatarID, Data.Avatars))
                return Ok();
            return BadRequest();
        }

        // GET api/avatars/1337/bookmarks?searchType=avatar
        [HttpGet("{AvatarID}/relationships")]
        public ActionResult<N2RelationshipsByAvatarIDQueryResult> GetAvatarRelationships(uint AvatarID, [FromQuery] string direction)
        {
            direction = direction.ToLowerInvariant().Trim();
            Func<AvatarIDToken, IEnumerable<AvatarRelationship>> func = (direction == "outgoing") ? avatarDataService.GetRelationshipsByAvatarID : avatarDataService.GetReverseRelationshipsByAvatarID;
            ActionResult <IEnumerable<AvatarRelationship>> result = GetObjectByID(func, (AvatarIDToken)AvatarID);
            if (result.Result is not null) return result.Result;
            IEnumerable<AvatarRelationship>? relationshipInfo = result.Value;
            if (relationshipInfo == default) return NotFound(AvatarID);
            return new N2RelationshipsByAvatarIDQueryResult(AvatarID,direction,relationshipInfo);
        }

        // GET api/avatars/1337/char
        [HttpGet("{AvatarID}/char")]
        public Task<ActionResult<TSODBChar>> GetCharacterFileByAvatarID(uint AvatarID) => 
            GetObjectByIDAsync(avatarDataService.GetCharacterByAvatarID, (AvatarIDToken)AvatarID);

        // POST api/avatars/1337/char
        [HttpPost("{AvatarID}/char")]
        public async Task<ActionResult> SetCharacterFileByAvatarIDAsync(uint AvatarID, [FromBody] TSODBChar CharacterFile)
        {
            if (await avatarDataService.SetCharacterByAvatarID(AvatarID, CharacterFile))
                return Ok();
            return BadRequest();
        }

        // POST api/avatars/1337/funds/set?Value=500000
        [HttpPost("{AvatarID}/funds/set")]
        public async Task<ActionResult<int>> SetFundsByAvatarID(uint AvatarID, int Value) => await avatarDataService.SetFundsByAvatarID(AvatarID, Value);

        // GET api/avatars/1337/online
        [HttpGet("{AvatarID}/online")]
        public ActionResult<bool> GetAvatarIsOnlineByAvatarID(uint AvatarID) => GetObjectByID(avatarDataService.GetAvatarOnlineStatus, (AvatarIDToken)AvatarID);

        // POST api/avatars/1337/online
        [HttpPost("{AvatarID}/online")]
        public async Task<ActionResult<bool>> SetAvatarIsOnlineByAvatarIDAsync(uint AvatarID, [FromQuery] bool IsOnline) => await avatarDataService.SetOnlineStatusByAvatarID(AvatarID, IsOnline);

        /// <summary>
        /// Creates a new Avatar, returns the <see cref="AvatarIDToken"/> of the newly created avatar, and takes in a method string describing what application is used to create the avatar
        /// </summary>
        /// <param name="User"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        // GET api/avatars/create
        [HttpGet("create")]
        public async Task<ActionResult<uint>> CreateNewAvatar([FromQuery] string user, [FromQuery] string method) => (uint)(await avatarDataService.CreateNewAvatar((UserToken)user, method));

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

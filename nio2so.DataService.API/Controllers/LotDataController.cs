using Microsoft.AspNetCore.Mvc;
using nio2so.Data.Common.Testing;
using nio2so.DataService.API.Databases;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Lot;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/lots")]
    [ApiController]
    public class LotDataController : DataServiceControllerBase
    {
        private ILogger<LotDataController> logger;

        LotsDataService lotsDataService => APIDataServices.LotDataService;

        public LotDataController(ILogger<LotDataController> Logger) : base()
        {
            logger = Logger;
        }

        // GET api/lots/profiles
        [HttpGet("profiles")]
        public ActionResult<N2GetLotListQueryResult> GetAllLotProfiles() => 
            new N2GetLotListQueryResult(TestingConstraints.MyShardName,
                lotsDataService.GetLots().Select(x => new N2GetLotListQueryResult.LotListEntry(x.HouseID,x.Position)));        

        // GET api/lots/1338/profile
        [HttpGet("{HouseID}/profile")]
        public ActionResult<LotProfile> GetLotProfile(uint HouseID) => GetObjectByID(lotsDataService.GetLotProfileByLotID, (HouseIDToken)HouseID);

        // GET api/lots/1338/roommates
        [HttpGet("{HouseID}/roommates")]
        public ActionResult<IEnumerable<AvatarIDToken>> GetRoommatesByHouseID(uint HouseID) => GetObjectByID(lotsDataService.GetRoommatesByHouseID, (HouseIDToken)HouseID);

        // GET api/lots/1338/profile?field=name
        [HttpPost("{HouseID}/profile")]
        public async Task<ActionResult> MutateProfile(uint HouseID, [FromQuery] string Field, [FromBody] string Value)
        {
            if (await lotsDataService.SetLotProfileFields(HouseID, Field, Value))
                return Accepted();
            return Unauthorized(Field);
        }

        // GET api/lots/purchase?AvatarID=1337&Phone=6548784&X=1&Y=1
        [HttpGet("purchase")]
        public async Task<ActionResult<LotProfile>> AvatarPurchaseLotByID(uint AvatarID, uint HouseID, uint X, uint Y)
        {
            Common.Types.Avatar.TSODBChar character = await APIDataServices.AvatarDataService.GetCharacterByAvatarID(AvatarID);
            (bool Success, string Reason, LotProfile? NewLotProfile) = await lotsDataService.TryPurchaseLotByAvatarID(AvatarID, HouseID, character, new LotPosition(X, Y));
            if (Success && NewLotProfile != default)
            {
                Success = await APIDataServices.AvatarDataService.SetCharacterByAvatarID(AvatarID, character);
                if (!Success)
                    return BadRequest("Could not update the Avatar Character File for: " + AvatarID);
                return NewLotProfile;
            }
            return BadRequest(Reason);
        }

        // GET api/lots/1338/thumbnail
        [HttpGet("{HouseID}/thumbnail")]
        public async Task GetThumbnail(uint HouseID)
        {
            try
            {
                byte[] png = await lotsDataService.GetThumbnailByHouseID(HouseID);
                await MIMEResponse(png, "image/png", 200);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;                
            }
        }

        // POST api/lots/1338/thumbnail
        [HttpPost("{HouseID}/thumbnail")]
        public async Task<ActionResult> PostThumbnail(uint HouseID)
        {
            if ((Request?.ContentLength ?? 0) == 0) return BadRequest("No request/content!!");
            if (Request.ContentType != "image/png") return BadRequest("MIME type accepted: image/png.");            
            byte[] pngBytes = new byte[Request.ContentLength.Value];
            await Request.Body.ReadExactlyAsync(pngBytes);
            await lotsDataService.SetThumbnailByHouseID(HouseID, pngBytes);
            return Accepted();
        }

        // GET api/houses/1338/blob
        [HttpGet("{HouseID}/blob")]
        public async Task GetHouseCharBlobAsync(uint HouseID) => 
            await MIMEResponse((await GetObjectByIDAsync(lotsDataService.GetHouseBlobByHouseID, (HouseIDToken)HouseID)).Value);
        // POST api/houses/1338/blob
        [HttpPost("{HouseID}/blob")]
        public async Task<ActionResult> SetHouseCharBlobAsync(uint HouseID)
        {
            if (Request.ContentType != "application/octet-stream")
                return StatusCode(StatusCodes.Status406NotAcceptable);
            byte[] Data = new byte[(int)Request.ContentLength];
            await Request.Body.ReadExactlyAsync(Data);
            try
            {
                await lotsDataService.SetHouseBlobByHouseID(HouseID, Data);
            }
            catch(KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch(InvalidOperationException ioe)
            {
                return Unauthorized(ioe.Message);
            }
            return Accepted();
        }

        // POST api/<LotDataController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LotDataController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LotDataController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

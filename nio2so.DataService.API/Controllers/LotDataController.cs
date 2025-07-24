using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
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
    public class LotDataController : ControllerBase
    {
        LotsDataService lotsDataService => APIDataServices.LotDataService;

        private ActionResult<T> getByLotIDBase<T>(Func<HouseIDToken, T> QueryFunction, HouseIDToken HouseID)
        {
            T? objectData = default;
            try
            {
                objectData = QueryFunction(HouseID);
            }
            catch (KeyNotFoundException ke)
            {
                return NotFound(HouseID);
            }
            catch (FileNotFoundException fe)
            {
                return NotFound(HouseID);
            }
            if (objectData == null)
                return NotFound(HouseID);
            return objectData;
        }

        // GET api/lots/profiles
        [HttpGet("profiles")]
        public ActionResult<N2GetLotListQueryResult> GetAllLotProfiles() => 
            new N2GetLotListQueryResult(TestingConstraints.MyShardName,
                lotsDataService.GetLots().Select(x => new N2GetLotListQueryResult.LotListEntry(x.HouseID,x.Position)));        

        // GET api/lots/1338/profile
        [HttpGet("{HouseID}/profile")]
        public ActionResult<LotProfile> GetLotProfile(uint HouseID) => getByLotIDBase(lotsDataService.GetLotProfileByLotID, HouseID);

        // GET api/lots/1338/profile?field=name
        [HttpPost("{HouseID}/profile")]
        public ActionResult MutateProfile(uint HouseID, [FromQuery] string Field, [FromBody] string Value)
        {
            if (lotsDataService.SetLotProfileFields(HouseID, Field, Value))
                return Accepted();
            return Unauthorized(Field);
        }

        // GET api/lots/purchase?AvatarID=1337&Phone=6548784&X=1&Y=1
        [HttpGet("purchase")]
        public ActionResult<LotProfile> AvatarPurchaseLotByID(uint AvatarID, string Phone, uint X, uint Y)
        {
            var character = APIDataServices.AvatarDataService.GetCharacterByAvatarID(AvatarID);
            if (lotsDataService.TryPurchaseLotByAvatarID(AvatarID, character, Phone, new LotPosition(X, Y), out LotProfile? NewLot))
            {
                APIDataServices.AvatarDataService.SetCharacterByAvatarID(AvatarID, character);
                return NewLot;
            }
            return BadRequest();
        }

        // GET api/lots/1338/thumbnail
        [HttpGet("{id}/thumbnail")]
        public async Task Get(uint HouseID)
        {
            try
            {
                byte[] png = await lotsDataService.GetThumbnailByHouseID(HouseID);

                Response.Headers.Add(HeaderNames.ContentType, "image/png");
                Response.StatusCode = 200;
                await Response.Body.WriteAsync(png);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;                
            }
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

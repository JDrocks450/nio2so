﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            return Ok();
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

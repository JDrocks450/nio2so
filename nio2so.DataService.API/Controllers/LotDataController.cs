using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using nio2so.DataService.API.Databases;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nio2so.DataService.API.Controllers
{
    [Route("api/lots")]
    [ApiController]
    public class LotDataController : ControllerBase
    {
        LotsDataService lotsDataService => APIDataServices.LotDataService;

        // GET: api/<LotDataController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;

namespace nio2so.TSOProtocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector InitialConnect.jsp resource
    /// </summary>
    [ApiController]
    [Route("/cityselector")]
    public class ShardStatusServletController : ControllerBase
    {
        private readonly ILogger<ShardStatusServletController> _logger;

        public ShardStatusServletController(ILogger<ShardStatusServletController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("shard-status.jsp")]
        public ActionResult Get()
        {
            _logger.LogInformation("Client requests Shard Status(es)...");
            var shardStatus = ShardStatusStructure.Default;
            var packetStr = new ShardStatusPacket(shardStatus).ToString();
            _logger.LogInformation($"CitySelector: ShardStatus() === \n {packetStr} \n===");
            return Ok(packetStr);
        }
    }
}

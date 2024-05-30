using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Protocol.Data.Credential;
using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;

namespace nio2so.TSOProtocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector InitialConnect.jsp resource
    /// </summary>
    [ApiController]
    [Route("/cityselector")]
    public class ShardSelectorServletController : ControllerBase
    {
        private readonly ILogger<ShardSelectorServletController> _logger;

        public ShardSelectorServletController(ILogger<ShardSelectorServletController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("shard-selector.jsp")]
        public ActionResult Get(string ShardName, string? AvatarID)
        {
            _logger.LogInformation("Client needs an available shard...");               
            _logger.LogInformation($"CitySelector: ShardSelector({ShardName}, {AvatarID}) invoked by Client.");
            bool isCasReq = false;
            ShardSelectionPacket? packet = null;            

            if (AvatarID == null)
            { // ENTERING CAS!
                AvatarID = 0x00A2.ToString();
                _logger.LogInformation($"CitySelector: Generated AvatarID and forwarding Client to CAS.");
                isCasReq = true;                
            }         
            else            
                _logger.LogInformation($"CitySelector: Sending to City ({ShardName})");

            if (!uint.TryParse(AvatarID, out uint PlayerID))
            {
                string errorMsg = $"The AvatarID {AvatarID ?? "null"} you gave me isn't uh, A NUMBER?";
                _logger.LogInformation("ERROR: " + errorMsg);
                return Ok(new ErrorMessagePacket(1337, errorMsg));
            }                               

            packet = new ShardSelectionPacket(new ShardSelectionStructure("localhost:49",TSOSessionTicket.CityDefault,uint.Parse(AvatarID)));
            _logger.LogInformation($"CitySelector: Returned === \n {packet.ToString()} \n ===");
            return Ok(packet.ToString());
        }
    }
}

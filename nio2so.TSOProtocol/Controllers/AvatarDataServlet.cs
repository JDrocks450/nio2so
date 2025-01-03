using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Data.Common.Testing;
using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;

namespace nio2so.TSOProtocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector InitialConnect.jsp resource
    /// </summary>
    [ApiController]
    [Route("/cityselector")]
    public class AvatarDataServletController : ControllerBase
    {
        private readonly ILogger<AvatarDataServletController> _logger;

        public AvatarDataServletController(ILogger<AvatarDataServletController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("avatar-data.jsp")]
        public ActionResult Get()
        {
            _logger.LogInformation("Client requests AvatarData...");
            var avatarData = AvatarDataPacket.Default;
            bool CASActivated = TestingConstraints.CASTestingMode;
            var packetStr = new AvatarDataPacket(avatarData).ToString();
            if (!System.IO.File.Exists(@$"E:\packets\avatar\avatar1337.charblob"))
                packetStr = new AvatarDataPacket().ToString();
            if (CASActivated) packetStr = new AvatarDataPacket().ToString();
            _logger.LogInformation($"CitySelector: AvatarData() === \n {packetStr} \n===");
            return Ok(packetStr);
        }
    }
}

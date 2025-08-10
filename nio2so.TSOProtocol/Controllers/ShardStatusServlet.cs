using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.DataService.Common.Tokens;
using nio2so.Formats;
using nio2so.TSOHTTPS.Protocol.Data;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PlayTest;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha;

namespace nio2so.TSOHTTPS.Protocol.Controllers
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
        public ActionResult ShardStatusServlet()
        {
            _logger.LogInformation("Client requests Shard Status(es)...");

            //**try to get version from the EntryLobby
            if (!Identify(out TSOVersions? GameVersion) || GameVersion == default)
                return BadRequest();

            //**get shard statuses from the version of the game we're using
            IVersionedPacketStructure shardStatus = GameVersion == TSOVersions.TSO_PreAlpha ? TSOPE_ShardStatusStructure.Default : ShardStatusStructure.Default;
            List<IVersionedPacketStructure> shards = new List<IVersionedPacketStructure>() { shardStatus };
            if (GameVersion != TSOVersions.TSO_PreAlpha)
            {
                shards.Clear();
                //for testing, make all cities online
                string[] mapNames = TSOStringProvider.GetCityNames();
                for (int index = 0; index < mapNames.Length; index++)
                {
                    int map = index + 1;
                    shards.Add(new ShardStatusStructure(mapNames[index % mapNames.Length], map, (uint)map, map));
                }
            }
            //return shard statuses to client
            var packetStr = new ShardStatusPacket(shards.ToArray()).ToString();
            _logger.LogInformation($"CitySelector: ShardStatus() === \n {packetStr} \n===");
            return Ok(packetStr);
        }

        /// <summary>
        /// Consults with the <see cref="EntryLobby"/> to find this connection's <see cref="UserToken"/>
        /// </summary>
        /// <param name="GameVersion"></param>
        /// <returns></returns>
        private bool Identify(out TSOVersions? GameVersion)
        {
            _logger.LogInformation($"{GetType().Name} is consulting with the lobby...");
            GameVersion = default;
            var session = Request.HttpContext.Connection;
            return EntryLobby.GetVersion(session.RemoteIpAddress, session.RemotePort, out GameVersion);
        }
    }
}

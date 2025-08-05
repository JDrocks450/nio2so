using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.Protocol.Data.Credential;
using nio2so.TSOHTTPS.Protocol.Data;
using nio2so.TSOHTTPS.Protocol.Services;
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
        private readonly nio2soMVCDataServiceClient dataService;
        private readonly ILogger<ShardSelectorServletController> _logger;

        public ShardSelectorServletController(nio2soMVCDataServiceClient dataService, ILogger<ShardSelectorServletController> logger)
        {
            this.dataService = dataService;
            _logger = logger;
        }

        /// <summary>
        /// Consults with the <see cref="EntryLobby"/> to find this connection's <see cref="UserToken"/>
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        private bool Identify(out UserToken UserToken)
        {
            _logger.LogInformation($"ShardSelectorServlet is consulting with the lobby...");
            UserToken = default;
            var session = Request.HttpContext.Connection;
            return EntryLobby.Serve(session.RemoteIpAddress, session.RemotePort, out UserToken); // REMOVE
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("shard-selector.jsp")]
        public async Task<ActionResult> Get(string ShardName, string? AvatarID)
        {
            _logger.LogInformation("Client needs an available shard...");

            if (!Identify(out UserToken UserToken))
                return BadRequest();

            _logger.LogInformation($"CitySelector: ShardSelector({ShardName}, {AvatarID}) invoked by Client.");
            bool isCasReq = false;
            ShardSelectionPacket? packet = null;

            if (AvatarID == null)
            { // ENTERING CAS!
                DataService.Common.HTTPServiceClientBase.HTTPServiceResult<uint> queryResult = await dataService.CreateNewAvatarFile(UserToken, "CAS_PREALPHA");
                uint? uniqueRemote = queryResult.Result;
                if (queryResult.IsSuccessful)
                    AvatarID = uniqueRemote.ToString();
                else throw new Exception(queryResult.FailureReason);
                _logger.LogInformation($"CitySelector: Generated AvatarID {AvatarID} and forwarding Client to CAS.");
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

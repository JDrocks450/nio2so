#define REMOVE_FROM_LOBBY
#undef REMOVE_FROM_LOBBY

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.DataService.Common.Tokens;
using nio2so.Formats;
using nio2so.TSOHTTPS.Protocol.Data;
using nio2so.TSOHTTPS.Protocol.Data.Credential;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PlayTest;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha;
using nio2so.TSOHTTPS.Protocol.Services;

namespace nio2so.TSOHTTPS.Protocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector <c>shard-selector.jsp</c> resource
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
        /// <para/>Normally this client should be removed, by this breaks rejoining to Select-A-Sim so keep them for now.
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        private bool Identify(out UserToken? UserToken)
        {
            _logger.LogInformation($"{nameof(ShardSelectorServletController)} is consulting with the lobby...");
            UserToken = default;
            var session = Request.HttpContext.Connection;
#if REMOVE_FROM_LOBBY
            return EntryLobby.Serve(session.RemoteIpAddress, session.RemotePort, out UserToken); // REMOVE
#else
            return EntryLobby.GetUser(session.RemoteIpAddress, session.RemotePort, out UserToken); // KEEP
#endif
        }

        /// <summary>
        /// Release builds of The Sims Online
        /// </summary>
        /// <param name="ShardName"></param>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        [HttpGet("app/ShardSelectorServlet")]
        public Task<ActionResult> GetShard_PlayTest(string ShardName, string? AvatarID) => ShardSelectorServlet(TSOVersions.TSO_PlayTest, ShardName, AvatarID);
        /// <summary>
        /// The Sims Online: Pre-Alpha clients
        /// </summary>
        /// <param name="ShardName"></param>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        [HttpGet("shard-selector.jsp")]
        public Task<ActionResult> GetShard_PreAlpha(string ShardName, string? AvatarID) => ShardSelectorServlet(TSOVersions.TSO_PreAlpha, ShardName, AvatarID);

        /// <summary>
        /// Returns an entry ticket to Voltron on a given server shard
        /// </summary>
        /// <returns></returns>        
        async Task<ActionResult> ShardSelectorServlet(TSOVersions Version, string ShardName, string? AvatarID)
        {
            _logger.LogInformation($"Client is connecting to {ShardName}...");

            //**identify this client by connection
            if (!Identify(out UserToken? UserToken) || !UserToken.HasValue)
                return BadRequest();

            _logger.LogInformation($"CitySelector: ShardSelector({ShardName}, {AvatarID}) invoked by Client.");
            ShardSelectionPacket? packet = null;

            //determine if we are creating an avatar or joining the city
            if (AvatarID == null)
            { // ENTERING CAS!
                DataService.Common.HTTPServiceClientBase.HTTPServiceResult<uint> queryResult = await dataService.CreateNewAvatarFile(UserToken.Value, "CAS_PREALPHA");
                uint? uniqueRemote = queryResult.Result;
                if (queryResult.IsSuccessful)
                    AvatarID = uniqueRemote.ToString();
                else throw new Exception(queryResult.FailureReason);
                _logger.LogInformation($"CitySelector: Generated AvatarID {AvatarID} and forwarding Client to CAS.");
            }
            else
                _logger.LogInformation($"CitySelector: User: {UserToken} Sent to City ({ShardName})");
            //can we parse this avatarID?
            if (!uint.TryParse(AvatarID, out uint NAvatarID))
            {
                string errorMsg = $"The AvatarID {AvatarID ?? "null"} is invalid. (Not a number)";
                _logger.LogError(errorMsg);
                return Ok(new ErrorMessagePacket(1337, errorMsg));
            }

            //connect to the data service to synchronize shard joining parameters
            DataService.Common.HTTPServiceClientBase.HTTPServiceResult<DataService.Common.Types.VoltronServerSettings> result = await dataService.GetVoltronServiceSettings();
            if (result == null || result.Result == null || !result.IsSuccessful)
                throw new InvalidOperationException(result?.FailureReason ?? "Result of the query VoltronSettings was null.");

            //connection details and new session ticket
            string ConnectionAddress = result.Result.ServerConnectionAddress;
            TSOSessionTicket Ticket = TSOSessionTicket.GetNext();
            uint ShardIndex = 1;

            //**respond with packet structure for selecting a shard
            ITSOXMLStructure structure = Version == TSOVersions.TSO_PreAlpha ? new TSOSE_ShardSelectionStructure(ConnectionAddress, Ticket, NAvatarID) :
                new ShardSelectionStructure(ConnectionAddress,Ticket,AvatarID,ShardIndex,0,NAvatarID);

            packet = new ShardSelectionPacket(structure);
            _logger.LogInformation($"CitySelector: Returned === \n {packet.ToString()} \n ===");
            return Ok(packet.ToString());
        }
    }
}

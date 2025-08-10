using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.DataService.Common.Tokens;
using nio2so.Formats;
using nio2so.TSOHTTPS.Protocol.Data;
using nio2so.TSOHTTPS.Protocol.Data.Credential;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML;

namespace nio2so.TSOHTTPS.Protocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector InitialConnect.jsp resource
    /// </summary>
    [ApiController]
    [Route("/cityselector")]
    public class InitialConnectServletController : ControllerBase
    {
        private const string USER_AGENT = "SimsOnline";

        private readonly ILogger<InitialConnectServletController> _logger;

        public InitialConnectServletController(ILogger<InitialConnectServletController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// For use with The Sims Online: Play-Test <para/>
        /// <inheritdoc cref="InitialConnect(string, string)"/>
        /// </summary>
        /// <param name="Ticket">The <see cref="UserToken"/> of the user</param>
        /// <param name="Version"></param>
        /// <returns></returns>
        /// <returns></returns>
        [HttpGet("app/InitialConnectServlet")]
        public ActionResult Get_PlayTest(string Ticket, string Version) => InitialConnect(Ticket, Version, TSOVersions.TSO_PlayTest);

        /// <summary>
        /// For use with The Sims Online: Pre-Alpha <para/>
        /// <inheritdoc cref="InitialConnect(string, string)"/>
        /// </summary>
        /// <param name="Ticket">The <see cref="UserToken"/> of the user</param>
        /// <param name="Version"></param>
        /// <returns></returns>
        [HttpGet("initial-connect.jsp")]
        public ActionResult Get_PreAlpha(string Ticket, string Version) => InitialConnect(Ticket, Version, TSOVersions.TSO_PreAlpha);
        /// <summary>
        /// Will return User-Authorized, Patch-Result and Error-Message depending on whether this Client is valid.
        /// </summary>
        /// <param name="Ticket">The <see cref="UserToken"/> of the user</param>
        /// <param name="Version"></param>
        /// <returns></returns>
        ActionResult InitialConnect(string Ticket, string Version, TSOVersions GameVersion)
        {
            _logger.LogInformation($"{TSOStringProvider.GetVersionName(GameVersion)} client is transitioning to the City Selector ({nameof(InitialConnect)})");
            int function = 1; // function 1 will accept an incoming connection
            TSOXMLPacket? returnPacket = null;

            //check version
            if (Version == "NEVERPATCH") // make the patch never show up unless deliberate
                function = 0; // patch available

            switch (function)
            {
                case 0: // PATCH RESULT
                    const string patchAddr = "https://nio2so.tso.ad.max.ea.com/patch1.0"; // web address, goes legit nowhere, used for debugging.
                    returnPacket = new PatchResultPacket(new Uri(patchAddr), new TSOSessionTicket(Ticket));
                    break;
                case 1: // GOOD 2 GO
                    returnPacket = Authorize(Ticket, GameVersion);
                    break;
                default: // ERROR
                    returnPacket = new ErrorMessagePacket(132, "Generic nio2so error.");
                    break;
            }
            _logger.LogInformation($"CitySelector: InitConnect({function}) === \n {returnPacket} \n===");
            return Ok(returnPacket.ToString());
        }

        private UserAuthorizePacket Authorize(UserToken Token, TSOVersions GameClientVersion)
        {
            var remote = Request.HttpContext.Connection;
            EntryLobby.Add(Token, remote.RemoteIpAddress, remote.RemotePort, GameClientVersion);
            return new UserAuthorizePacket();
        }
    }
}

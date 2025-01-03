using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Protocol.Data.Credential;
using nio2so.TSOProtocol.Packets.TSOXML;
using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;

namespace nio2so.Protocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector InitialConnect.jsp resource
    /// </summary>
    [ApiController]
    [Route("/cityselector")]
    public class InitialConnectServletController : ControllerBase
    {
        private readonly ILogger<InitialConnectServletController> _logger;

        public InitialConnectServletController(ILogger<InitialConnectServletController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Will return User-Authorized, Patch-Result and Error-Message depending on whether this Client is valid.
        /// </summary>
        /// <param name="Ticket"></param>
        /// <param name="Version"></param>
        /// <returns></returns>
        [HttpGet("initial-connect.jsp")]
        public ActionResult Get(string Ticket, string Version)
        {
            _logger.LogInformation("TSOClient transitioning to the City Selector (Initial-Connect)");
            int function = 1; // function 1 will accept an incoming connection
            TSOXMLPacket? returnPacket = null;

            //check version
            if (Version == "NEVERPATCH") // make the patch never show up unless deliberate
                function = 0; // patch available

            switch (function)
            {
                case 0: // PATCH RESULT
                    const string patchAddr = "https://nio2so.tso.ad.max.ea.com/patch1.0"; // web address, goes legit nowhere, used for debugging.
                    returnPacket = new PatchResultPacket(new Uri(patchAddr), TSOSessionTicket.LoginDefault);
                    break;
                case 1: // GOOD 2 GO
                    returnPacket = new UserAuthorizePacket();
                    break;
                default: // ERROR
                    returnPacket = new ErrorMessagePacket(132, "Generic nio2so error.");
                    break;
            }
            _logger.LogInformation($"CitySelector: InitConnect({function}) === \n {returnPacket} \n===");
            return Ok(returnPacket.ToString());
        }
    }
}

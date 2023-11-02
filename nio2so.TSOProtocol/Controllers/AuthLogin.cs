using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Protocol.Data.Credential;
using nio2so.Protocol.Packets;

namespace nio2so.Protocol.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthLoginController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<AuthLoginController> _logger;

        public AuthLoginController(ILogger<AuthLoginController> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        public ActionResult Get(string? username, string? password, int ServiceID, string? Version)
        {
            _logger.LogInformation("TSOClient attempting connection. ");
            AuthReasonPacket.AuthErrors errorCode = AuthReasonPacket.AuthErrors.INV000; // NO ERROR
            if (Version != "2.5")            
                errorCode = AuthReasonPacket.AuthErrors.INV123; // PLAY RIGHTS EXPIRED
            if (username != "bloaty") // USERNAME INVALID!
                errorCode = AuthReasonPacket.AuthErrors.INV110;
            if (username == "") // USERNAME/PASSWORD BLANK!
                errorCode = AuthReasonPacket.AuthErrors.INV020;  
            AuthReasonPacket reason = default;
            string returnPacketText = "";
            if (errorCode != AuthReasonPacket.AuthErrors.INV000)            
                reason = AuthReasonPacket.MakeInvalid(errorCode);            
            else
                reason = AuthReasonPacket.MakeSuccessful(TSOSessionTicket.LoginDefault);                            
            returnPacketText = reason.ToString();
            _logger.LogInformation($"TSOClient AuthRequestResponse =====\n{returnPacketText}\n=====");
            return Ok(returnPacketText);
        }
    }
}
#define LOGIN_ERRORS_TEST
#undef LOGIN_ERRORS_TEST

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Data.Common.Testing;
using nio2so.Protocol.Data.Credential;
using nio2so.Protocol.Packets;

namespace nio2so.Protocol.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthLoginController : ControllerBase
    {
        private readonly ILogger<AuthLoginController> _logger;

        public AuthLoginController(ILogger<AuthLoginController> logger)
        {
            _logger = logger;
        }

#if LOGIN_ERRORS_TEST
        static int currentValue = 0;
#endif

        [HttpGet()]
        public ActionResult Get(string? username, string? password, int ServiceID, string? Version)
        {
            _logger.LogInformation("TSOClient attempting connection. ");
            AuthReasonPacket.AuthErrors errorCode = AuthReasonPacket.AuthErrors.INV000; // NO ERROR
            if (Version != "2.5")            
                errorCode = AuthReasonPacket.AuthErrors.INV123; // PLAY RIGHTS EXPIRED
            if (string.IsNullOrWhiteSpace(username)) // USERNAME/PASSWORD BLANK!
                errorCode = AuthReasonPacket.AuthErrors.INV020;  
            else if (username != TestingConstraints.LoginUsername) // USERNAME INVALID!
                errorCode = AuthReasonPacket.AuthErrors.INV110;            
            AuthReasonPacket reason = default;
            string returnPacketText = "";
#if LOGIN_ERRORS_TEST
            currentValue++;
            errorCode = Enum.GetValues<AuthReasonPacket.AuthErrors>()[currentValue];
#endif
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
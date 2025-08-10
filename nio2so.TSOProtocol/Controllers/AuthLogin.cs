#define LOGIN_ERRORS_TEST
#undef LOGIN_ERRORS_TEST

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.DataService.Common.Tokens;
using nio2so.TSOHTTPS.Protocol.Data.Credential;
using nio2so.TSOHTTPS.Protocol.Packets;
using nio2so.TSOHTTPS.Protocol.Services;

namespace nio2so.TSOHTTPS.Protocol.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthLoginController : ControllerBase
    {
        private readonly nio2soMVCDataServiceClient dataServiceClient;
        private readonly ILogger<AuthLoginController> _logger;

        public AuthLoginController(nio2soMVCDataServiceClient dataServiceClient, ILogger<AuthLoginController> logger)
        {
            this.dataServiceClient = dataServiceClient;
            _logger = logger;
        }

#if LOGIN_ERRORS_TEST
        static int currentValue = 0;
#endif

        [HttpGet()]
        public async Task<ActionResult<string>> Get(string? username, string? password, int ServiceID, string? Version)
        {
            _logger.LogInformation("TSOClient attempting connection. ");            

            var reason = await AuthLogin(username, password, ServiceID, Version);
                                    
            string returnPacketText = reason.ToString();
            _logger.LogInformation($"TSOClient AuthRequestResponse =====\n{returnPacketText}\n=====");
            return returnPacketText;
        }

        private async Task<AuthReasonPacket> AuthLogin(string? username, string? password, int ServiceID, string? Version)
        {
            AuthReasonPacket.AuthErrors errorCode = PreCheck(username, Version);

#if LOGIN_ERRORS_TEST
            currentValue++;
            errorCode = Enum.GetValues<AuthReasonPacket.AuthErrors>()[currentValue];
#endif

            if (errorCode != AuthReasonPacket.AuthErrors.INV000)
                return AuthReasonPacket.MakeInvalid(errorCode);

            //** ping nio2so data service
            //get usertoken by username
            UserToken? token = await dataServiceClient.GetUserTokenByUserName(username);

            if (!token.HasValue) // not found
                return AuthReasonPacket.MakeInvalid(AuthReasonPacket.AuthErrors.INV110);
            //found account
            return AuthReasonPacket.MakeSuccessful(new TSOSessionTicket(token));
        }

        private AuthReasonPacket.AuthErrors PreCheck(string? username, string? Version)
        {
            AuthReasonPacket.AuthErrors errorCode = AuthReasonPacket.AuthErrors.INV000; // NO ERROR
            if (Version != "2.5")
                errorCode = AuthReasonPacket.AuthErrors.INV123; // PLAY RIGHTS EXPIRED
            if (string.IsNullOrWhiteSpace(username)) // USERNAME/PASSWORD BLANK!
                errorCode = AuthReasonPacket.AuthErrors.INV020;
            //else if (username != TestingConstraints.LoginUsername) // USERNAME INVALID!
            //  errorCode = AuthReasonPacket.AuthErrors.INV110; 
            return errorCode;
        }
    }
}
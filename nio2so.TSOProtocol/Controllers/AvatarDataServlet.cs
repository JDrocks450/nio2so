using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.TSOHTTPS.Protocol.Data;
using nio2so.TSOHTTPS.Protocol.Services;
using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;

namespace nio2so.TSOProtocol.Controllers
{
    /// <summary>
    /// This controller will handle requests to the CitySelector Avatar-Data.jsp resource
    /// </summary>
    [ApiController]
    [Route("/cityselector")]
    public class AvatarDataServletController : ControllerBase
    {
        private readonly nio2soMVCDataServiceClient dataServiceClient;
        private readonly ILogger<AvatarDataServletController> _logger;

        public AvatarDataServletController(nio2soMVCDataServiceClient dataServiceClient, ILogger<AvatarDataServletController> logger)
        {
            this.dataServiceClient = dataServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("avatar-data.jsp")]
        public async Task<ActionResult<string>> GetAsync()
        {
            if (!Identify(out UserToken User))
                return BadRequest();

            _logger.LogInformation($"{User} requests AvatarData...");

            //**download user info avatar list from nio2so data service
            AvatarIDToken[] avatars = await DownloadAvatarList(User);
            //**download avatar profile for each avatar from nio2so dms
            AvatarProfile[] avatarProfiles = await DownloadAvatarProfileListAsync(avatars);

            _logger.LogInformation($"TSOHTTPS received {avatarProfiles.Length} AvatarProfiles from nio2so...");

            //**build response
            var packetStr = new AvatarDataPacket(avatarProfiles).ToString();

            _logger.LogInformation($"CitySelector: {User} AvatarData() === \n {packetStr} \n===");
            return packetStr;
        }        

        /// <summary>
        /// Consults with the <see cref="EntryLobby"/> to find this connection's <see cref="UserToken"/>
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        private bool Identify(out UserToken UserToken)
        {
            _logger.LogInformation($"AvatarDataServlet is consulting with the lobby...");
            UserToken = default;
            var session = Request.HttpContext.Connection;
            return EntryLobby.Serve(session.RemoteIpAddress, session.RemotePort, out UserToken);
        }

        /// <summary>
        /// Gets all <see cref="AvatarProfile"/> data for the <paramref name="avatars"/> provided
        /// </summary>
        /// <param name="avatars"></param>
        /// <returns></returns>
        private async Task<AvatarProfile[]> DownloadAvatarProfileListAsync(AvatarIDToken[] avatars)
        {
            _logger.LogInformation($"{User} requests AvatarProfiles({avatars.Length})...");
            List<AvatarProfile> returnList = new List<AvatarProfile>();
            foreach (var avatar in avatars)
            {
                //**download data from nio2so data service
                AvatarProfile? resp = await DownloadAvatarProfile(avatar);
                if (resp != null)
                    returnList.Add(resp);
            }
            return returnList.ToArray();
        }

        /// <summary>
        /// <inheritdoc cref="nio2soMVCDataServiceClient.GetAvatarProfileByAvatarID(AvatarIDToken)"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        private Task<AvatarProfile?> DownloadAvatarProfile(AvatarIDToken AvatarID) => dataServiceClient.GetAvatarProfileByAvatarID(AvatarID);

        /// <summary>
        /// Download the list of avatars belonging to this <paramref name="UserAccount"/>
        /// </summary>
        /// <param name="UserAccount"></param>
        /// <returns>On not found, returns empty list</returns>
        private async Task<AvatarIDToken[]> DownloadAvatarList(UserToken UserAccount)
        {
            if (TestingConstraints.CASTestingMode) return [];

            var responseBody = await dataServiceClient.GetUserInfoByUserToken(UserAccount);
            if (responseBody == null) return [];
            return responseBody.Avatars;
        }
    }
}

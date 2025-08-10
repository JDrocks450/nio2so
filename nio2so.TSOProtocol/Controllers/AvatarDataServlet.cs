using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats;
using nio2so.TSOHTTPS.Protocol.Data;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PlayTest;
using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha;
using nio2so.TSOHTTPS.Protocol.Services;
using static nio2so.DataService.Common.HTTPServiceClientBase;

namespace nio2so.TSOHTTPS.Protocol.Controllers
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
        /// For use with Release Builds of The Sims Online Play-Test
        /// </summary>
        /// <returns></returns>
        [HttpGet("app/AvatarDataServlet")]
        public Task<ActionResult<string>> Get_PlayTest() => AvatarDataServlet(TSOVersions.TSO_Release);
        /// <summary>
        /// For use with The Sims Online Pre Alpha
        /// </summary>
        /// <returns></returns>
        [HttpGet("avatar-data.jsp")]
        public Task<ActionResult<string>> Get_PreAlpha() => AvatarDataServlet(TSOVersions.TSO_PreAlpha);

        /// <summary>
        /// For use with The Sims Online: Pre-Alpha and The Sims Online: Play-Test
        /// </summary>
        /// <returns></returns>                
        async Task<ActionResult<string>> AvatarDataServlet(TSOVersions GameVersion)
        {
            _logger.LogWarning($"{nameof(AvatarDataServlet)} Context: " + TSOStringProvider.GetVersionName(GameVersion));

            if (!Identify(out UserToken? User) || !User.HasValue)
                return BadRequest();

            _logger.LogInformation($"{User} requests AvatarData...");

            //**download user info avatar list from nio2so data service
            AvatarIDToken[] avatars = await DownloadAvatarList(User.Value);
            //**download avatar profile for each avatar from nio2so dms
            AvatarProfile[] avatarProfiles = await DownloadAvatarProfileListAsync(avatars);

            _logger.LogInformation($"TSOHTTPS received {avatarProfiles.Length} AvatarProfiles from nio2so...");

            //**build response
            string packetStr = GameVersion == TSOVersions.TSO_PreAlpha ? BuildXMLDocument_PreAlpha(avatarProfiles) : BuildXMLDocument_PlayTest(avatarProfiles);

            _logger.LogInformation($"CitySelector: {User} {AvatarDataServlet}() === \n {packetStr} \n===");
            return packetStr;
        }        
        /// <summary>
        /// Makes a response packet formatted for The Sims Online: Pre-Alpha clients using <see cref="TSOPE_AvatarDataStructure"/>
        /// </summary>
        /// <param name="Profiles"></param>
        /// <returns></returns>
        string BuildXMLDocument_PreAlpha(params AvatarProfile[] Profiles) => new AvatarDataPacket<TSOPE_AvatarDataStructure>(
                [.. Profiles.Select(static x =>
                    new TSOPE_AvatarDataStructure(AvatarID: x.AvatarID, Name: x.Name, Simoleans: x.Simoleans,
                                                  SimoleanDelta: x.SimoleanDelta, Popularity: x.Popularity,
                                                  PopularityDelta: x.PopularityDelta, ShardName: x.ShardName))],
                    "Player-Active").ToString();
        /// <summary>
        /// Makes a response packet formatted for The Sims Online: Play-test clients using <see cref="AvatarDataPacketStructure"/>
        /// </summary>
        /// <param name="Profiles"></param>
        /// <returns></returns>
        string BuildXMLDocument_PlayTest(params AvatarProfile[] Profiles) => new AvatarDataPacket<AvatarDataPacketStructure>(
                [.. Profiles.Select(static x => new AvatarDataPacketStructure(AvatarID: x.AvatarID, Name: x.Name, ShardName: x.ShardName))]).ToString();

        /// <summary>
        /// Consults with the <see cref="EntryLobby"/> to find this connection's <see cref="UserToken"/>
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        private bool Identify(out UserToken? UserToken)
        {
            _logger.LogInformation($"{GetType().Name} is consulting with the lobby...");
            UserToken = default;
            var session = Request.HttpContext.Connection;
            return EntryLobby.GetUser(session.RemoteIpAddress, session.RemotePort, out UserToken);
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
                if (avatar == 0) continue;
                //**download data from nio2so data service
                HTTPServiceResult<AvatarProfile> resp = await DownloadAvatarProfile(avatar);
                if (resp.IsSuccessful && resp.Result != null)
                    returnList.Add(resp.Result);
            }
            return returnList.ToArray();
        }

        /// <summary>
        /// <inheritdoc cref="nio2soMVCDataServiceClient.GetAvatarProfileByAvatarID(AvatarIDToken)"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        private Task<HTTPServiceResult<AvatarProfile>> DownloadAvatarProfile(AvatarIDToken AvatarID) => dataServiceClient.GetAvatarProfileByAvatarID(AvatarID);

        /// <summary>
        /// Download the list of avatars belonging to this <paramref name="UserAccount"/>
        /// </summary>
        /// <param name="UserAccount"></param>
        /// <returns>On not found, returns empty list</returns>
        private async Task<AvatarIDToken[]> DownloadAvatarList(UserToken UserAccount)
        {
            if (TestingConstraints.CASTestingMode) return [];

            var responseBody = await dataServiceClient.GetUserInfoByUserToken(UserAccount);
            if (!responseBody.IsSuccessful || responseBody.Result?.Avatars == default) return [];
            return responseBody.Result.Avatars;
        }
    }
}

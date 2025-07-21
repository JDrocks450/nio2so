using Microsoft.Extensions.Configuration;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOHTTPS.Protocol.Services
{
    public class nio2soDataServiceClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        private Uri APIAddress => new Uri(_configuration.GetValue<string>("APIAddress"));

        private HttpResponseMessage? _lastResponse;

        public nio2soDataServiceClient(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        private async Task<T?> baseQueryGetAs<T>(string Query)
        {
            var uri = new Uri(APIAddress, Query);
            var response = _lastResponse = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();               
            }
            else
            {
                return default;
            }
        }

        public async Task<UserToken?> GetUserTokenByUserName(string UserName)
        {
            var response = await baseQueryGetAs<N2AccountByUserNameQueryResult>("account/" + UserName);
            return response?.ServerUserToken;
        }
        public Task<UserInfo?> GetUserInfoByUserToken(UserToken Account) => baseQueryGetAs<UserInfo>("users/" + Account);
        public Task<AvatarProfile?> GetAvatarProfileByAvatarID(AvatarIDToken AvatarID) => baseQueryGetAs<AvatarProfile>($"avatars/{AvatarID.AvatarID}/profile");
    }
}

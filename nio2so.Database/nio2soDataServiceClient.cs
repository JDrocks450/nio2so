using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using System.Net.Http.Json;

namespace nio2so.TSOHTTPS.Protocol.Services
{
    /// <summary>
    /// Interface for interacting with the nio2so Data Service to make requests
    /// </summary>
    public class nio2soDataServiceClient
    {
        private readonly HttpClient _client;
        private HttpResponseMessage? _lastResponse;        
        
        /// <summary>
        /// Creates a new <see cref="nio2soDataServiceClient"/>
        /// <para><inheritdoc cref="APIAddress"/></para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="configuration"></param>
        public nio2soDataServiceClient(HttpClient client, Uri nio2soApiBaseAddress)
        {
            _client = client;
            _client.BaseAddress = nio2soApiBaseAddress;
        }

        private async Task<T?> baseQueryGetAs<T>(string Query)
        {
            var response = _lastResponse = await _client.GetAsync(Query);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();               
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Gets the <see cref="UserToken"/> by the given <paramref name="UserName"/>
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public async Task<UserToken?> GetUserTokenByUserName(string UserName)
        {
            var response = await baseQueryGetAs<N2AccountByUserNameQueryResult>("account/" + UserName);
            return response?.ServerUserToken;
        }
        /// <summary>
        /// Downloads the <see cref="UserInfo"/> for the given <see cref="UserToken"/> <paramref name="Account"/>
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        public Task<UserInfo?> GetUserInfoByUserToken(UserToken Account) => baseQueryGetAs<UserInfo>("users/" + Account);
        /// <summary>
        /// Downloads the <see cref="AvatarProfile"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<AvatarProfile?> GetAvatarProfileByAvatarID(AvatarIDToken AvatarID) => baseQueryGetAs<AvatarProfile>($"avatars/{AvatarID.AvatarID}/profile");
    }
}

using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using System.Net.Http.Json;
using System.Text;

namespace nio2so.DataService.Common
{
    /// <summary>
    /// Interface for interacting with the nio2so Data Service to make requests/responses
    /// </summary>
    public class nio2soDataServiceClient
    {
        /// <summary>
        /// The inner <see cref="HttpClient"/> to use for manual logic if needed.
        /// <para/>You need to dispose this yourself
        /// </summary>
        protected HttpClient Client { get; }
        /// <summary>
        /// The response from the last call to <see cref="baseQueryGetAs{T}(string)"/>
        /// </summary>
        protected HttpResponseMessage? LastResponse { get; private set; }       
        
        /// <summary>
        /// Creates a new <see cref="nio2soDataServiceClient"/>
        /// <para><inheritdoc cref="APIAddress"/></para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="configuration"></param>
        public nio2soDataServiceClient(HttpClient client, Uri nio2soApiBaseAddress)
        {
            Client = client;
            Client.BaseAddress = nio2soApiBaseAddress;
        }

        protected async Task<HttpResponseMessage> httpGet(string Query, params (string Name, object Value)[] Parameters)
        {
            StringBuilder query = new(Query);
            int count = 0;
            foreach (var parameter in Parameters)
            {
                query.Append((count == 0 ? "?" : "&") + $"{parameter.Name}={parameter.Value}");
                count++;
            }

            return LastResponse = await Client.GetAsync(query.ToString());
        }

        protected async Task<T?> baseQueryGetAs<T>(string Query, params (string Name, object Value)[] Parameters)
        {
            var response = await httpGet(Query,Parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();               
            }
            else
            {
                return default;
            }
        }
        protected async Task<byte[]?> baseQueryGetOctet(string Query, params (string Name, object Value)[] Parameters)
        {
            var response = await httpGet(Query, Parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                return default;
            }
        }
        protected async Task<HttpResponseMessage> baseQueryPostAs<T>(string Query, T Post, params (string Name, object Value)[] Parameters)
        {
            StringBuilder query = new(Query);
            int count = 0;
            foreach (var parameter in Parameters)
            {
                query.Append((count == 0 ? "?" : "&") + $"{parameter.Name}={parameter.Value}");
                count++;
            }

            return LastResponse = await Client.PostAsJsonAsync(query.ToString(), Post);            
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
        /// <summary>
        /// Downloads the bookmarks for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<N2BookmarksByAvatarIDQueryResult?> GetAvatarBookmarksByAvatarID(AvatarIDToken AvatarID) => 
            baseQueryGetAs<N2BookmarksByAvatarIDQueryResult>($"avatars/{AvatarID.AvatarID}/bookmarks",("listType","avatar"));
        
        /// <summary>
        /// Uploads the bookmarks for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public async Task<bool> SetAvatarBookmarksByAvatarID(AvatarIDToken AvatarID, params AvatarIDToken[] Avatars) {

            var response = await baseQueryPostAs($"avatars/{AvatarID}/bookmarks", new N2BookmarksByAvatarIDQueryResult(AvatarID, Avatars), ("listType", "avatar"));
            return response.IsSuccessStatusCode;
        }
        /// <summary>
        /// Downloads the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<TSODBChar?> GetCharacterFileByAvatarID(AvatarIDToken AvatarID) =>
            baseQueryGetAs<TSODBChar>($"avatars/{AvatarID.AvatarID}/char");

        /// <summary>
        /// Uploads the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public async Task<bool> SetCharacterFileByAvatarID(AvatarIDToken AvatarID, TSODBChar CharacterFile)
        { 
            var response = await baseQueryPostAs($"avatars/{AvatarID}/char", CharacterFile);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Creates a new Avatar File and returns the ID of the new Avatar
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<uint?> CreateNewAvatarFile(UserToken User, string Method) =>
            baseQueryGetAs<uint?>($"avatars/create", ("user", User), ("method", Method));

        /// <summary>
        /// Downloads the <see cref="TSODBCharBlob"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<byte[]?> GetAvatarCharBlobByAvatarID(AvatarIDToken AvatarID) =>
            baseQueryGetOctet($"avatars/{AvatarID.AvatarID}/appearance");

        /// <summary>
        /// Uploads the new <see cref="TSODBCharBlob"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public async Task<bool> SetAvatarCharBlobByAvatarID(AvatarIDToken AvatarID, byte[] CharBlobStream)
        {
            using MemoryStream stream = new MemoryStream(CharBlobStream);
            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var response = await Client.PostAsync($"avatars/{AvatarID}/appearance",content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Downloads the PNG Image for the given <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<byte[]?> GetThumbnailByHouseID(HouseIDToken HouseID) =>
            baseQueryGetOctet($"lots/{HouseID.HouseID}/thumbnail");

        /// <summary>
        /// Returns the name of the avatar by the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public async Task<string> GetAvatarNameByAvatarID(AvatarIDToken AvatarID) =>
            await (await httpGet($"avatars/{AvatarID}/name")).Content.ReadAsStringAsync();

        /// <summary>
        /// Returns a list of the <see cref="HouseIDToken"/> and <see cref="LotPosition"/> of all lots in this Shard
        /// </summary>
        /// <returns></returns>
        public Task<N2GetLotListQueryResult?> GetAllLotProfiles() =>
            baseQueryGetAs<N2GetLotListQueryResult>("lots/profiles");

        /// <summary>
        /// Returns a <see cref="LotProfile"/> by the given <see cref="HouseIDToken"/> <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public Task<LotProfile?> GetLotProfileByHouseID(HouseIDToken HouseID) =>
            baseQueryGetAs<LotProfile>($"lots/{HouseID}/profile");

        /// <summary>
        /// Changes a field on the <see cref="LotProfile"/> to be the new value
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> MutateLotProfileField(HouseIDToken HouseID, string Field, string Value) =>
            baseQueryPostAs($"lots/{HouseID}/profile", Value, (nameof(Field), Field));

        /// <summary>
        /// Attempts to purchase a new slot in the map. Returns a <see cref="LotProfile"/> containing the new <see cref="HouseIDToken"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>Null when purchasing was not successful.</returns>
        public Task<LotProfile?> AttemptToPurchaseLotByAvatarID(AvatarIDToken AvatarID, string Phone, uint X, uint Y) =>
            baseQueryGetAs<LotProfile>($"lots/purchase", (nameof(AvatarID),AvatarID), (nameof(Phone), Phone), (nameof(X), X), (nameof(Y), Y));
    }
}

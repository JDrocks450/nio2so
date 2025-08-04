using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using System.Net.Http.Json;
using System.Text;

namespace nio2so.DataService.Common
{
    public abstract class HTTPServiceClientBase
    {
        /// <summary>
        /// Creates a new <see cref="HTTPServiceClientBase"/>
        /// <para><inheritdoc cref="APIAddress"/></para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="configuration"></param>
        public HTTPServiceClientBase(HttpClient client, Uri baseAddress)
        {
            Client = client;
            Client.BaseAddress = baseAddress;
        }

        /// <summary>
        /// The inner <see cref="HttpClient"/> to use for manual logic if needed.
        /// <para/>You need to dispose this yourself
        /// </summary>
        protected HttpClient Client { get; }
        /// <summary>
        /// The response from the last call to <see cref="baseQueryGetAs{T}(string)"/>
        /// </summary>
        protected HttpResponseMessage? LastResponse { get; private set; }


        public static string BuildQueryString(string Resource, params (string Name, object Value)[] Parameters)
        {
            StringBuilder query = new(Resource);
            int count = 0;
            foreach (var parameter in Parameters)
            {
                query.Append((count == 0 ? "?" : "&") + $"{parameter.Name}={parameter.Value}");
                count++;
            }
            return query.ToString();
        }

        public async Task<HttpResponseMessage> HttpGet(string Query, params (string Name, object Value)[] Parameters) => 
            LastResponse = await Client.GetAsync(BuildQueryString(Query, Parameters));

        public async Task<T?> GetQueryAs<T>(string Query, params (string Name, object Value)[] Parameters)
        {
            var response = await HttpGet(Query, Parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            else
            {
                return default;
            }
        }
        public async Task<byte[]?> GetQueryAsOctet(string Query, params (string Name, object Value)[] Parameters)
        {
            var response = await HttpGet(Query, Parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                return default;
            }
        }
        public async Task<HttpResponseMessage> QueryPostAs<T>(string Query, T Post, params (string Name, object Value)[] Parameters) => 
            LastResponse = await Client.PostAsJsonAsync(BuildQueryString(Query, Parameters), Post);
        public async Task<HttpResponseMessage> QueryPostAsOctet(string Query, byte[] OctetStream, string MIMEType, params (string Name, object Value)[] Parameters)
        {
            using MemoryStream ms = new(OctetStream);
            StreamContent content = new StreamContent(ms);
            content.Headers.Add("Content-Type", MIMEType);
            return LastResponse = await Client.PostAsync(BuildQueryString(Query, Parameters), content);
        }
    }

    /// <summary>
    /// Interface for interacting with the nio2so Data Service to make requests/responses
    /// </summary>
    public class nio2soDataServiceClient : HTTPServiceClientBase
    {
        /// <summary>
        /// Creates a new <see cref="nio2soDataServiceClient"/>
        /// <para><inheritdoc cref="APIAddress"/></para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="configuration"></param>
        public nio2soDataServiceClient(HttpClient client, Uri nio2soApiBaseAddress) : base(client, nio2soApiBaseAddress) { }

        /// <summary>
        /// Gets the <see cref="UserToken"/> by the given <paramref name="UserName"/>
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public async Task<UserToken?> GetUserTokenByUserName(string UserName)
        {
            var response = await GetQueryAs<N2AccountByUserNameQueryResult>("account/" + UserName);
            return response?.ServerUserToken;
        }
        /// <summary>
        /// Downloads the <see cref="UserInfo"/> for the given <see cref="UserToken"/> <paramref name="Account"/>
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        public Task<UserInfo?> GetUserInfoByUserToken(UserToken Account) => GetQueryAs<UserInfo>("users/" + Account);
        /// <summary>
        /// Downloads the <see cref="AvatarProfile"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<AvatarProfile?> GetAvatarProfileByAvatarID(AvatarIDToken AvatarID) => GetQueryAs<AvatarProfile>($"avatars/{AvatarID.AvatarID}/profile");
        /// <summary>
        /// Downloads the bookmarks for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<N2BookmarksByAvatarIDQueryResult?> GetAvatarBookmarksByAvatarID(AvatarIDToken AvatarID) => 
            GetQueryAs<N2BookmarksByAvatarIDQueryResult>($"avatars/{AvatarID.AvatarID}/bookmarks",("listType","avatar"));
        
        /// <summary>
        /// Uploads the bookmarks for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public async Task<bool> SetAvatarBookmarksByAvatarID(AvatarIDToken AvatarID, params AvatarIDToken[] Avatars) {

            var response = await QueryPostAs($"avatars/{AvatarID}/bookmarks", new N2BookmarksByAvatarIDQueryResult(AvatarID, Avatars), ("listType", "avatar"));
            return response.IsSuccessStatusCode;
        }
        /// <summary>
        /// Downloads the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<TSODBChar?> GetCharacterFileByAvatarID(AvatarIDToken AvatarID) =>
            GetQueryAs<TSODBChar>($"avatars/{AvatarID.AvatarID}/char");

        /// <summary>
        /// Uploads the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public async Task<bool> SetCharacterFileByAvatarID(AvatarIDToken AvatarID, TSODBChar CharacterFile)
        { 
            var response = await QueryPostAs($"avatars/{AvatarID}/char", CharacterFile);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Creates a new Avatar File and returns the ID of the new Avatar
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<uint?> CreateNewAvatarFile(UserToken User, string Method) =>
            GetQueryAs<uint?>($"avatars/create", ("user", User), ("method", Method));

        /// <summary>
        /// Downloads the <see cref="TSODBCharBlob"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<byte[]?> GetAvatarCharBlobByAvatarID(AvatarIDToken AvatarID) =>
            GetQueryAsOctet($"avatars/{AvatarID.AvatarID}/appearance");

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
            GetQueryAsOctet($"lots/{HouseID.HouseID}/thumbnail");
        /// <summary>
        /// Uploads the PNG Image for the given <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        public async Task<bool> SetHouseThumbnailByID(HouseIDToken houseID, byte[] pngBytes) =>
            (await QueryPostAsOctet($"lots/{houseID}/thumbnail", pngBytes, "image/png")).IsSuccessStatusCode;

        /// <summary>
        /// Returns the name of the avatar by the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public async Task<string> GetAvatarNameByAvatarID(AvatarIDToken AvatarID) =>
            await (await HttpGet($"avatars/{AvatarID}/name")).Content.ReadAsStringAsync();

        /// <summary>
        /// Returns a list of the <see cref="HouseIDToken"/> and <see cref="LotPosition"/> of all lots in this Shard
        /// </summary>
        /// <returns></returns>
        public Task<N2GetLotListQueryResult?> GetAllLotProfiles() =>
            GetQueryAs<N2GetLotListQueryResult>("lots/profiles");

        /// <summary>
        /// Returns a <see cref="LotProfile"/> by the given <see cref="HouseIDToken"/> <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public Task<LotProfile?> GetLotProfileByHouseID(HouseIDToken HouseID) =>
            GetQueryAs<LotProfile>($"lots/{HouseID}/profile");

        /// <summary>
        /// Changes a field on the <see cref="LotProfile"/> to be the new value
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> MutateLotProfileField(HouseIDToken HouseID, string Field, string Value) =>
            QueryPostAs($"lots/{HouseID}/profile", Value, (nameof(Field), Field));

        /// <summary>
        /// Attempts to purchase a new slot in the map. Returns a <see cref="LotProfile"/> containing the new <see cref="HouseIDToken"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>Null when purchasing was not successful.</returns>
        public Task<LotProfile?> AttemptToPurchaseLotByAvatarID(AvatarIDToken AvatarID, uint HouseID, uint X, uint Y) =>
            GetQueryAs<LotProfile>($"lots/purchase", (nameof(AvatarID),AvatarID), (nameof(HouseID), HouseID), (nameof(X), X), (nameof(Y), Y));
        /// <summary>
        /// Attempts to purchase a new slot in the map. Returns a <see cref="LotProfile"/> containing the new <see cref="HouseIDToken"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>Null when purchasing was not successful.</returns>
        public Task<IEnumerable<AvatarIDToken>?> GetRoommatesByHouseID(HouseIDToken HouseID) =>
            GetQueryAs<IEnumerable<AvatarIDToken>>($"lots/{HouseID}/roommates");
        /// <summary>
        /// Searches for the given resource <paramref name="Type"/> exactly matching the given search <paramref name="Query"/>
        /// </summary>
        /// <returns></returns>
        public async Task<N2SearchQueryResult> SubmitSearchExact(string Query, string Type) =>
            await GetQueryAs<N2SearchQueryResult>($"search/{Type}/exact",(nameof(Query),Query)) ?? new(Query, Type, []);
        /// <summary>
        /// Searches for the given resource <paramref name="Type"/> broadly matching the given search <paramref name="Query"/>
        /// </summary>
        /// <returns></returns>
        public async Task<N2SearchQueryResult> SubmitSearch(string Query, string Type) =>
            await GetQueryAs<N2SearchQueryResult>($"search/{Type}", (nameof(Query), Query)) ?? new(Query, Type, []);
        /// <summary>
        /// Gets how the <see cref="AvatarIDToken"/> <paramref name="AvatarID"/> feels about other avatars
        /// </summary>
        /// <returns></returns>
        public async Task<N2RelationshipsByAvatarIDQueryResult?> GetOutgoingRelationshipsByAvatarID(AvatarIDToken AvatarID) =>
            await GetQueryAs<N2RelationshipsByAvatarIDQueryResult>($"avatars/{AvatarID}/relationships", ("direction", "outgoing"));
        /// <summary>
        /// Gets how the other avatars feel about <see cref="AvatarIDToken"/> <paramref name="AvatarID"/> 
        /// </summary>
        /// <returns></returns>
        public async Task<N2RelationshipsByAvatarIDQueryResult?> GetIncomingRelationshipsByAvatarID(AvatarIDToken AvatarID) =>
            await GetQueryAs<N2RelationshipsByAvatarIDQueryResult>($"avatars/{AvatarID}/relationships", ("direction", "incoming"));
    }
}

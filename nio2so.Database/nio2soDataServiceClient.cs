using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using static nio2so.DataService.Common.HTTPServiceClientBase;

namespace nio2so.DataService.Common
{
    public abstract class HTTPServiceClientBase
    {
        public class HTTPServiceResult<T>
        {
            public HttpStatusCode StatusCode { get; set; }
            public bool IsSuccessful => (200 >= (int)StatusCode) && ((int)StatusCode < 300);
            public string FailureReason { get; set; } = "none set.";
            public T? Result { get; set; }

            public HTTPServiceResult(HttpStatusCode StatusCode, T? Result) {
                this.StatusCode = StatusCode;
                this.Result = Result;
            }
            public HTTPServiceResult(HttpStatusCode StatusCode, string? FailureReason, T? Result = default) : this(StatusCode,Result)
            {
                this.FailureReason = FailureReason ?? this.FailureReason;
            }
        }

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

        async Task<HTTPServiceResult<T>> BaseGetQueryAs<T>(string Query, Func<HttpContent, Task<T>> BodyFunction, params (string Name, object Value)[] Parameters)
        {
            var response = await HttpGet(Query, Parameters);
            string? failureReason = default;
            T? result = default;
            if (response.IsSuccessStatusCode)
                result = await BodyFunction(response.Content);
            else
                failureReason = await response.Content.ReadAsStringAsync();
            failureReason = $"[{GetType().Name}] GET {BuildQueryString(Query, Parameters)}: ({(int)response.StatusCode} {response.StatusCode}) {failureReason}";
            return new HTTPServiceResult<T>(response.StatusCode, failureReason, result);
        }

        public Task<HTTPServiceResult<T>> GetQueryAs<T>(string Query, params (string Name, object Value)[] Parameters) => 
            BaseGetQueryAs(Query, content => content.ReadFromJsonAsync<T>(), Parameters);
        public Task<HTTPServiceResult<byte[]>> GetQueryAsOctet(string Query, params (string Name, object Value)[] Parameters) => 
            BaseGetQueryAs(Query, content => content.ReadAsByteArrayAsync(), Parameters);
        public async Task<HttpResponseMessage> QueryPostAs<T>(string Query, T Post, params (string Name, object Value)[] Parameters) => 
            LastResponse = await Client.PostAsJsonAsync(BuildQueryString(Query, Parameters), Post);
        public async Task<HttpResponseMessage> QueryPost(string Query, HttpContent Content, params (string Name, object Value)[] Parameters) =>
            LastResponse = await Client.PostAsync(BuildQueryString(Query, Parameters), Content);
        public async Task<HttpResponseMessage> QueryPostAsString(string Query, string PostContent, params (string Name, object Value)[] Parameters) =>
            LastResponse = await Client.PostAsync(BuildQueryString(Query, Parameters), new StringContent(PostContent));
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
            return response?.Result?.ServerUserToken;
        }
        /// <summary>
        /// Downloads the <see cref="UserInfo"/> for the given <see cref="UserToken"/> <paramref name="Account"/>
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        public Task<HTTPServiceResult<UserInfo>> GetUserInfoByUserToken(UserToken Account) => GetQueryAs<UserInfo>("users/" + Account);
        /// <summary>
        /// Downloads the <see cref="AvatarProfile"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<HTTPServiceResult<AvatarProfile>> GetAvatarProfileByAvatarID(AvatarIDToken AvatarID) => GetQueryAs<AvatarProfile>($"avatars/{AvatarID.AvatarID}/profile");
        /// <summary>
        /// Downloads the bookmarks for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<HTTPServiceResult<N2BookmarksByAvatarIDQueryResult>> GetAvatarBookmarksByAvatarID(AvatarIDToken AvatarID) => 
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
        public Task<HTTPServiceResult<TSODBChar>> GetCharacterFileByAvatarID(AvatarIDToken AvatarID) =>
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
        public Task<HTTPServiceResult<uint>> CreateNewAvatarFile(UserToken User, string Method) =>
            GetQueryAs<uint>($"avatars/create", ("user", User), ("method", Method));

        /// <summary>
        /// Downloads the <see cref="TSODBCharBlob"/> for the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<HTTPServiceResult<byte[]>> GetAvatarCharBlobByAvatarID(AvatarIDToken AvatarID) =>
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
        /// Downloads the <see cref="TSODBHouseBlob"/> for the given <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<HTTPServiceResult<byte[]>> GetHouseBlobByHouseID(HouseIDToken HouseID) =>
            GetQueryAsOctet($"lots/{HouseID}/blob");

        /// <summary>
        /// Uploads the new <see cref="TSODBHouseBlob"/> for the given <paramref name="HouseID"/>
        /// <para/>Please observe that a lot can be marked as ReadOnly, in which this will fail.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>On not found, returns null</returns>
        public async Task<(bool Result, string Reason)> SetHouseBlobByHouseID(HouseIDToken HouseID, byte[] HouseBlobStream)
        {
            using MemoryStream stream = new MemoryStream(HouseBlobStream);
            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var response = await Client.PostAsync($"lots/{HouseID}/blob", content);
            return (response.IsSuccessStatusCode, !response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "");
        }

        /// <summary>
        /// Downloads the PNG Image for the given <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>On not found, returns null</returns>
        public Task<HTTPServiceResult<byte[]>> GetThumbnailByHouseID(HouseIDToken HouseID) =>
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
        public Task<HTTPServiceResult<N2GetLotListQueryResult>> GetAllLotProfiles() =>
            GetQueryAs<N2GetLotListQueryResult>("lots/profiles");

        /// <summary>
        /// Returns a <see cref="LotProfile"/> by the given <see cref="HouseIDToken"/> <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public Task<HTTPServiceResult<LotProfile>> GetLotProfileByHouseID(HouseIDToken HouseID) =>
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
        public Task<HTTPServiceResult<LotProfile>> AttemptToPurchaseLotByAvatarID(AvatarIDToken AvatarID, uint HouseID, uint X, uint Y) =>
            GetQueryAs<LotProfile>($"lots/purchase", (nameof(AvatarID),AvatarID), (nameof(HouseID), HouseID), (nameof(X), X), (nameof(Y), Y));
        /// <summary>
        /// Attempts to purchase a new slot in the map. Returns a <see cref="LotProfile"/> containing the new <see cref="HouseIDToken"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns>Null when purchasing was not successful.</returns>
        public Task<HTTPServiceResult<IEnumerable<AvatarIDToken>>> GetRoommatesByHouseID(HouseIDToken HouseID) =>
            GetQueryAs<IEnumerable<AvatarIDToken>>($"lots/{HouseID}/roommates");
        /// <summary>
        /// Searches for the given resource <paramref name="Type"/> exactly matching the given search <paramref name="Query"/>
        /// </summary>
        /// <returns></returns>
        public async Task<N2SearchQueryResult> SubmitSearchExact(string Query, string Type) =>
            (await GetQueryAs<N2SearchQueryResult>($"search/{Type}/exact",(nameof(Query),Query))).Result ?? new(Query, Type, []);
        /// <summary>
        /// Searches for the given resource <paramref name="Type"/> broadly matching the given search <paramref name="Query"/>
        /// </summary>
        /// <returns></returns>
        public async Task<N2SearchQueryResult> SubmitSearch(string Query, string Type) =>
            (await GetQueryAs<N2SearchQueryResult>($"search/{Type}", (nameof(Query), Query))).Result ?? new (Query, Type, []);
        /// <summary>
        /// Gets how the <see cref="AvatarIDToken"/> <paramref name="AvatarID"/> feels about other avatars
        /// </summary>
        /// <returns></returns>
        public async Task<HTTPServiceResult<N2RelationshipsByAvatarIDQueryResult>> GetOutgoingRelationshipsByAvatarID(AvatarIDToken AvatarID) =>
            await GetQueryAs<N2RelationshipsByAvatarIDQueryResult>($"avatars/{AvatarID}/relationships", ("direction", "outgoing"));
        /// <summary>
        /// Gets how the other avatars feel about <see cref="AvatarIDToken"/> <paramref name="AvatarID"/> 
        /// </summary>
        /// <returns></returns>
        public async Task<HTTPServiceResult<N2RelationshipsByAvatarIDQueryResult>> GetIncomingRelationshipsByAvatarID(AvatarIDToken AvatarID) =>
            await GetQueryAs<N2RelationshipsByAvatarIDQueryResult>($"avatars/{AvatarID}/relationships", ("direction", "incoming"));
        /// <summary>
        /// Returns the online status of the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public Task<HTTPServiceResult<bool>> GetOnlineStatusByAvatarID(AvatarIDToken AvatarID) =>
            GetQueryAs<bool>($"avatars/{AvatarID}/online");

        /// <summary>
        /// Sets the online status of the given <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SetOnlineStatusByAvatarID(AvatarIDToken AvatarID, bool IsOnline) =>
            QueryPostAsString($"avatars/{AvatarID}/online","", (nameof(IsOnline),IsOnline));
    }
}

using nio2so.Data.Common.Testing;
using nio2so.DataService.API.Controllers;
using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Search;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Avatar information:
    /// <list type="bullet">Profile information of the Avatar shown in SAS</list>
    /// </summary>
    internal class AvatarDataService : DataServiceBase, ISearchable<uint>
    {
        const string charblob_folder = @"charblob";
        const string AvatarLibName = "AVATARS";

        private JSONDictionaryLibrary<uint, AvatarInfo> AvatarsLibrary => GetLibrary<JSONDictionaryLibrary<uint, AvatarInfo>>(AvatarLibName);

        internal AvatarDataService() : base() { }

        protected override void AddLibraries()
        {
            string fPath = ServerSettings.Current.AvatarInfoFile;

            Libraries.Add(AvatarLibName, 
                new JSONDictionaryLibrary<uint, AvatarInfo>(fPath, EnsureDefaultValues));

            Libraries.Add("BLOBS",
                new FileObjectLibrary(Path.Combine(Path.GetDirectoryName(fPath), charblob_folder), "avatar", "charblob", CharBlobNotFound));

            base.AddLibraries();
        }

        void EnsureDefaultValues()
        {
            ServerSettings settings = ServerSettings.Current;
            // not needed
        }       

        Task<byte[]> CharBlobNotFound() => File.ReadAllBytesAsync(ServerSettings.Current.DefaultCharblobPath);

        /// <summary>
        /// Returns the <see cref="AvatarProfile"/> for the given <paramref name="AvatarID"/>. The profile has no personal details and requires no validation **public info**
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public AvatarProfile GetProfileByAvatarID(AvatarIDToken AvatarID)
        {
            void correctErrors(AvatarProfile Profile)
            {
                if (Profile.AvatarID != AvatarID)
                {
                    if (Profile.AvatarID != 0)
                        throw new InvalidDataException($"The returned AvatarProfile for {AvatarID} belongs to {Profile.AvatarID}...?");
                }
            }
            var DataFile = GetLibrary<JSONDictionaryLibrary<uint, AvatarInfo>>(AvatarLibName);
            if (!DataFile.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            AvatarProfile profile = DataFile[AvatarID].Profile;
            correctErrors(profile);
            return profile;
        }
        /// <summary>
        /// Returns the name of the Avatar with the given ID
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public string GetNameByID(AvatarIDToken AvatarID) => GetProfileByAvatarID(AvatarID).Name;

        /// <summary>
        /// Returns the <see cref="AvatarInfo.AvatarBookmarkInfo"/> for the given <paramref name="AvatarID"/>. Bookmarks are public info, like a follower list on social media **public info**
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Common.Types.AvatarInfo.AvatarBookmarkInfo GetBookmarksByAvatarID(AvatarIDToken AvatarID)
        {
            void correctErrors(Common.Types.AvatarInfo.AvatarBookmarkInfo Bookmark)
            {
                // TODO
            }
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var bookmark = AvatarsLibrary[AvatarID].BookmarkInfo;
            correctErrors(bookmark);
            return bookmark;
        }

        /// <summary>
        /// Updates the list of bookmarks attached to this <paramref name="AvatarID"/> to be <paramref name="Avatars"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="Avatars"></param>
        /// <returns></returns>
        public bool SetBookmarksByAvatarID(AvatarIDToken AvatarID, IEnumerable<AvatarIDToken> Avatars)
        {
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                return false;            
            AvatarsLibrary[AvatarID].BookmarkInfo.BookmarkAvatars = new(Avatars);
            Save();
            return true;
        }
        /// <summary>
        /// Returns the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>. This is vital to the social aspect of the game. **public info**
        /// <para/>This will reevaluate fields on the <see cref="TSODBChar"/> and save the updated values to the disk.
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TSODBChar GetCharacterByAvatarID(AvatarIDToken AvatarID)
        {
            void Reevaluate(TSODBChar CharData)
            {
                //**get updated lot membership value
                CharData.MyLotID = APIDataServices.LotDataService.GetRoommateHouseIDByAvatarID(AvatarID) ?? 0;

                // TODO
                return;
                CharData.Unknown1 = 0;
                CharData.Unknown3 = 0;
                CharData.Unknown4 = 0;
                CharData.Unknown6 = 0;
            }
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var file = AvatarsLibrary[AvatarID].AvatarCharacter;
            Reevaluate(file);
            Save();
            return file;
        }

        /// <summary>
        /// Updates the <see cref="TSODBChar"/> attached to this <paramref name="AvatarID"/> to be <paramref name="CharacterFile"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="Avatars"></param>
        /// <returns></returns>
        public bool SetCharacterByAvatarID(AvatarIDToken AvatarID, TSODBChar CharacterFile)
        {
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                return false;
            AvatarsLibrary[AvatarID].AvatarCharacter = CharacterFile;
            Save();
            return true;
        }

        /// <summary>
        /// Returns the <see cref="TSODBCharBlob"/> for the given <see cref="AvatarIDToken"/> in binary
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task<byte[]> GetCharBlobByAvatarID(AvatarIDToken AvatarID) => GetLibrary<FileObjectLibrary>("BLOBS").GetDataByID(AvatarID);
        /// <summary>
        /// Sets the <see cref="TSODBCharBlob"/> for the given <see cref="AvatarIDToken"/> in binary
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task SetCharBlobByAvatarID(AvatarIDToken AvatarID, byte[] CharBlobStream, bool overwrite = true) =>
            GetLibrary<FileObjectLibrary>("BLOBS").SetDataByIDToDisk(AvatarID,CharBlobStream,overwrite);

        /// <summary>
        /// Creates a new <see cref="AvatarInfo"/> file with a new unique <see cref="AvatarIDToken"/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public AvatarIDToken CreateNewAvatar(UserToken user, string method)
        {
            AvatarInfo newInfo;
            uint AvatarID = 0;
            do
            {
                int one = Random.Shared.Next(100, int.MaxValue);
                int two = Random.Shared.Next(1, int.MaxValue);
                AvatarID = Math.Min((uint)(one + two), uint.MaxValue);
            }
            while (!AvatarsLibrary.TryAdd(AvatarID, newInfo = new AvatarInfo(user, AvatarID)
            {                
                CreatedUsing = method
            }));
            newInfo.AvatarCharacter.Funds = TestingConstraints.StartingFunds;
            Save();
            //add the avatar to this account
            if (!APIDataServices.UserDataService.AddAvatarToAccount(user, AvatarID, out _))
                throw new InvalidOperationException("Tried to link an avatar to an account that is full.");
            return AvatarID;
        }

        public bool DebitCreditTransaction(int AccountChange)
        {
            return true;
        }

        public IDictionary<uint,string> SearchExact(string QueryString) => AvatarsLibrary.SearchExact(QueryString);
        public IDictionary<uint, string> SearchBroad(string QueryString, int MaxResults) => AvatarsLibrary.SearchBroad(QueryString, MaxResults);
    }
}

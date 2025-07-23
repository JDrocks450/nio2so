using nio2so.Data.Common.Testing;
using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Avatar information:
    /// <list type="bullet">Profile information of the Avatar shown in SAS</list>
    /// </summary>
    internal class AvatarDataService : DataServiceBase
    {
        const string charblob_folder = @"charblob";
        const string ProfileLibName = "PROFILES";

        private JSONDictionaryLibrary<uint, AvatarInfo> ProfilesLibrary => GetLibrary<JSONDictionaryLibrary<uint, AvatarInfo>>(ProfileLibName);

        internal AvatarDataService() : base() { }

        protected override void AddLibraries()
        {
            string dir = ServerSettings.Current.AvatarInfoFile;

            Libraries.Add(ProfileLibName, 
                new JSONDictionaryLibrary<uint, AvatarInfo>(dir, EnsureDefaultValues));

            Libraries.Add("BLOBS",
                new FileObjectLibrary(Path.Combine(dir, charblob_folder), "avatar", "charblob", CharBlobNotFound));

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
            var DataFile = GetLibrary<JSONDictionaryLibrary<uint, AvatarInfo>>(ProfileLibName);
            if (!DataFile.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            AvatarProfile profile = DataFile[AvatarID].Profile;
            correctErrors(profile);
            return profile;
        }

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
            if (!ProfilesLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var bookmark = ProfilesLibrary[AvatarID].BookmarkInfo;
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
            if (!ProfilesLibrary.ContainsKey(AvatarID))
                return false;            
            ProfilesLibrary[AvatarID].BookmarkInfo.BookmarkAvatars = new(Avatars);
            Save();
            return true;
        }
        /// <summary>
        /// Returns the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>. This is vital to the social aspect of the game. **public info**
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TSODBChar GetCharacterByAvatarID(AvatarIDToken AvatarID)
        {
            void correctErrors(TSODBChar CharData)
            {
                // TODO
                //CHEATS!!!
                CharData.Funds = TestingConstraints.StaticFunds; // set beta funds level here
                return;
                CharData.Unknown1 = 0;
                CharData.MyLotID = 1;
                CharData.Unknown3 = 0;
                CharData.Unknown4 = 0;
                CharData.Unknown6 = 0;
            }
            if (!ProfilesLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var file = ProfilesLibrary[AvatarID].AvatarCharacter;
            correctErrors(file);
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
            if (!ProfilesLibrary.ContainsKey(AvatarID))
                return false;
            ProfilesLibrary[AvatarID].AvatarCharacter = CharacterFile;
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
            uint AvatarID = 0;
            do
            {
                int one = Random.Shared.Next(100, int.MaxValue);
                int two = Random.Shared.Next(1, int.MaxValue);
                AvatarID = Math.Min((uint)(one + two), uint.MaxValue);
            }
            while (!ProfilesLibrary.TryAdd(AvatarID, new AvatarInfo(user, AvatarID)
            {                
                CreatedUsing = method
            }));
            Save();
            //add the avatar to this account
            if (!APIDataServices.UserDataService.AddAvatarToAccount(user, AvatarID, out _))
                throw new InvalidOperationException("Tried to link an avatar to an account that is full.");
            return AvatarID;
        }
    }
}

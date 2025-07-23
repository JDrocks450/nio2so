using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Avatar information:
    /// <list type="bullet">Profile information of the Avatar shown in SAS</list>
    /// </summary>
    internal class AvatarDataService : DatabaseComponentBase<DataComponentDictionaryDataFile<uint, AvatarInfo>>
    {
        const string charblob_folder = @"charblob";

        internal AvatarDataService() : 
            base(ServerSettings.Current.AvatarInfoFile)
        {
            CreateDefaultValues();
        }

        protected override void CreateDefaultValues()
        {
            ServerSettings settings = ServerSettings.Current;
            // not needed
        }       

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
            if (!DataFile.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var bookmark = DataFile[AvatarID].BookmarkInfo;
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
            if (!DataFile.ContainsKey(AvatarID))
                return false;            
            DataFile[AvatarID].BookmarkInfo.BookmarkAvatars = new(Avatars);
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
            if (!DataFile.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var file = DataFile[AvatarID].AvatarCharacter;
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
            if (!DataFile.ContainsKey(AvatarID))
                return false;
            DataFile[AvatarID].AvatarCharacter = CharacterFile;
            Save();
            return true;
        }

        /// <summary>
        /// Returns the <see cref="TSODBCharBlob"/> for the given <see cref="AvatarIDToken"/> in binary
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task<byte[]> GetCharBlobByAvatarID(AvatarIDToken AvatarID)
        {
            string url = Path.Combine(BaseDirectory, charblob_folder, $"{AvatarID}.charblob");
            if (!File.Exists(url))
                throw new FileNotFoundException(url);
            return File.ReadAllBytesAsync(url);
        }
        /// <summary>
        /// Sets the <see cref="TSODBCharBlob"/> for the given <see cref="AvatarIDToken"/> in binary
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task SetCharBlobByAvatarID(AvatarIDToken AvatarID, byte[] CharBlobStream, bool overwrite = true)
        {
            Directory.CreateDirectory(Path.Combine(BaseDirectory, charblob_folder));
            string url = Path.Combine(BaseDirectory, charblob_folder, $"{AvatarID}.charblob");
            if (File.Exists(url) && !overwrite)
                throw new InvalidOperationException(url + " already exists!");
            File.Delete(url);
            return File.WriteAllBytesAsync(url,CharBlobStream);
        }

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
            while (!DataFile.TryAdd(AvatarID, new AvatarInfo(user, AvatarID)
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

using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Account information:
    /// <list type="bullet">What avatars you own</list>
    /// </summary>
    internal class UserDataService : DataServiceBase
    {
        public HashSet<string> ReservedUserNames { get; set; } = ["SYSTEM","RESERVED","NIO2SO","NIOTSO"];

        private JSONDictionaryLibrary<string, UserInfo> UsersLibrary => GetLibrary<JSONDictionaryLibrary<string, UserInfo>>("USERS");

        internal UserDataService() : base() { }

        protected override void AddLibraries()
        {
            Libraries.Add("USERS", new JSONDictionaryLibrary<string, UserInfo>(CurrentSettings.DereferencePath(CurrentSettings.UsersFile), CreateDefaultValues));
            base.AddLibraries();
        }

        async Task CreateDefaultValues()
        {
            ServerSettings settings = CurrentSettings;
            await CreateUserInfoFile(settings.StaticAccounts[0]); // ensure only
            await CreateUserInfoFile(settings.StaticAccounts[1]); // ensure only
        }

        /// <summary>
        /// Creates (or overwrites) the <see cref="UserInfo"/> file attached to the user <paramref name="UserAccount"/>
        /// </summary>
        /// <param name="UserAccount"></param>
        /// <param name="ExistingFile">If passed, will copy the properties from this parameter into the new <see cref="UserInfo"/> to update the existing file to be this new one.</param>
        /// <returns></returns>
        public async Task<(bool result, UserInfo? NewAccount)> CreateUserInfoFile(UserToken UserAccount)
        {
            UserInfo newFile = new(UserAccount);

            //add/update
            bool result = UsersLibrary.TryAdd(UserAccount, newFile);
            if (result)
                await Save(); // save
            return (result, newFile);
        }
        /// <summary>
        /// Returns the <see cref="UserInfo"/> file in the DB attached to this <paramref name="UserID"/>
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<UserInfo> GetUserInfoByUserToken(UserToken UserID)
        {
            if (UsersLibrary.TryGetValue(UserID, out UserInfo? userInfo))
            {
                if (userInfo == null)
                {
                    Console.WriteLine($"UserInfo for {UserID} was null! Immediately creating a new file... this may indicate data has been lost.");
                    (bool result, UserInfo? NewAccount) = await CreateUserInfoFile(UserID);
                    return NewAccount;
                }
                return userInfo;
            }
            throw new KeyNotFoundException($"The user {UserID} does not exist in nio2so.");
        }
        /// <summary>
        /// Attempts to add an AvatarID to the User's account to use in SAS.
        /// <para/>Please watch out for these failure conditions:
        /// <list type="bullet">You have 3 avatars already</list>
        /// <list type="bullet">Your account doesn't exist</list>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="avatarID"></param>
        /// <param name="AvatarIndex">The slot that the new avatar has been placed in.</param>
        /// <returns>True when added successfully.</returns>
        public async Task<(bool Result, Common.Types.AvatarInfo.AvatarSlots? AvatarIndex)> AddAvatarToAccount(UserToken user, uint avatarID)
        {
            Common.Types.AvatarInfo.AvatarSlots? AvatarIndex = null;
            UserInfo userInfo = await GetUserInfoByUserToken(user);
            for (int i = 0; i < 3; i++)
            {
                AvatarIDToken existing = userInfo.Avatars[i];
                if (existing != 0) continue;
                userInfo.Avatars[i] = avatarID;
                AvatarIndex = (AvatarInfo.AvatarSlots?)i;
                break;
            }
            return (AvatarIndex != null, AvatarIndex);
        }
    }
}

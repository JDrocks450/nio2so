using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Account information:
    /// <list type="bullet">What avatars you own</list>
    /// </summary>
    internal class UserDataService : DatabaseComponentBase<DataComponentDictionaryDataFile<string, UserInfo>>
    {
        public HashSet<string> ReservedUserNames { get; set; }

        internal UserDataService() : base(ServerSettings.Current.UsersFile)
        {
            CreateDefaultValues();
        }

        protected override void CreateDefaultValues()
        {
            ServerSettings settings = ServerSettings.Current;
            CreateUserInfoFile(settings.StaticAccounts[0], new UserInfo(new(), 1337));
            CreateUserInfoFile(settings.StaticAccounts[1], new UserInfo(new(), 161 ));
        }

        /// <summary>
        /// Creates (or overwrites) the <see cref="UserInfo"/> file attached to the user <paramref name="UserAccount"/>
        /// </summary>
        /// <param name="UserAccount"></param>
        /// <param name="ExistingFile">If passed, will copy the properties from this parameter into the new <see cref="UserInfo"/> to update the existing file to be this new one.</param>
        /// <returns></returns>
        public UserInfo CreateUserInfoFile(UserToken UserAccount, UserInfo? ExistingFile = null)
        {
            UserInfo newFile;
            if (ExistingFile != null)
                newFile = new UserInfo(UserAccount, ExistingFile);
            else newFile = new UserInfo(UserAccount);

            //add/update factory
            DataFile.AddOrUpdate(UserAccount, newFile, 
                delegate(string t, UserInfo existing) 
                { 
                    newFile.Copy(existing); // copy all existing properties to the new file **overwrite**
                    return newFile; 
                }
            );
            Save(); // save
            return newFile;
        }
        /// <summary>
        /// Returns the <see cref="UserInfo"/> file in the DB attached to this <paramref name="UserID"/>
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public UserInfo GetUserInfoByUserToken(UserToken UserID)
        {
            UserInfo bugCheckHandler()
            {
                Console.WriteLine($"UserInfo for {UserID} was null! Immediately creating a new file... this may indicate data has been lost.");
                return CreateUserInfoFile(UserID);
            }
            if (DataFile.TryGetValue(UserID, out UserInfo? userInfo))
                return userInfo ?? bugCheckHandler();
            throw new KeyNotFoundException($"The user {UserID} does not exist in nio2so.");
        }
    }
}

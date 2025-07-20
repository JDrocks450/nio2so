using nio2so.Database.Types;
using nio2so.DataService.Common.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.API.Databases
{
    internal class UserDataService : DatabaseComponentBase<DataComponentDictionaryDataFile<UserToken, UserInfo>>
    {
        public HashSet<string> ReservedUserNames { get; set; }

        internal UserDataService() : base(ServerSettings.Current.UsersFile)
        {
        
        }

        protected override void CreateDefaultValues()
        {
            DataFile.Add(1234, new UserInfo(1234, "bloaty", new() { 1337 }));
            DataFile.Add(4321, new UserInfo(4321, "friendly", new() { 161 }));
        }

        public UserInfo GetUserInfoByUserToken(UserToken UserID)
        {
            if (DataFile.TryGetValue(UserID, out UserInfo userInfo))
                return userInfo;
            throw new KeyNotFoundException($"The user {UserID} does not exist in nio2so.");
        }
    }
}

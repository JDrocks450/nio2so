using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Validates Login Credentials and serves the UserToken for the user trying to login
    /// </summary>
    internal class AccountsDataService : DatabaseComponentBase<DataComponentDictionaryDataFile<string,UserToken>>
    {        

        internal AccountsDataService() : base(ServerSettings.Current.AccountsFile)
        {

        }

        protected override void CreateDefaultValues()
        {
            DataFile.Add("bloaty", 1234);
            DataFile.Add("friendly", 4321);
        }

        public UserToken GetUserTokenByUserName(string UserName)
        {
            if (DataFile.TryGetValue(UserName, out UserToken accID))
                return accID;
            throw new KeyNotFoundException($"The UserName {UserName} does not exist in nio2so.");
        }
    }
}

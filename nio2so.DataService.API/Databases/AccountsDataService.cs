using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Validates Login Credentials and serves the UserToken for the user trying to login
    /// </summary>
    internal class AccountsDataService : DataServiceBase
    {
        private JSONDictionaryLibrary<string, UserToken> AccountsLibrary => GetLibrary<JSONDictionaryLibrary<string, UserToken>>("ACCOUNTS");

        internal AccountsDataService() : base()
        {
            
        }

        protected override void AddLibraries()
        {
            Libraries.Add("ACCOUNTS", new JSONDictionaryLibrary<string, UserToken>(ServerSettings.Current.AccountsFile, CreateDefaultValues));
            base.AddLibraries();
        }

        /// <summary>
        /// Ensures these default static accounts are present in the <see cref="AccountsDataService"/>
        /// </summary>
        void CreateDefaultValues()
        {
            foreach (var account in ServerSettings.Current.StaticAccounts)            
                EnsureAccount(account);
            Save();
        }

        /// <summary>
        /// Creates a new Account in the database and returns the <see cref="UserToken"/> of the newly created account
        /// </summary>
        /// <param name="UserName"></param>        
        /// <returns></returns>
        public UserToken CreateAccount(string UserName)
        {
            UserToken newToken = default;
            int failcount = 0, failmax = 5;
        retry:
            if (failcount >= failmax) throw new Exception("Internal server error.");
            do
            {
                newToken = UserToken.Create(UserName);
            }
            while (AccountsLibrary.ContainsKey(newToken)); // generate new unused key
            if (!AccountsLibrary.TryAdd(UserName, newToken)) // added by another concurrent thread
            {
                failcount++; // retry with a fresh user account token
                goto retry;
            }
            Save();
            return newToken;
        }

        /// <summary>
        /// Ensures an Account matches this <see cref="UserToken"/> in the database and returns if it was created or not
        /// </summary>
        /// </param>
        /// <returns></returns>
        public bool EnsureAccount(UserToken Token)
        {
            UserToken newToken = Token;
            return AccountsLibrary.TryAdd(Token.UserName, newToken);
        }

        public UserToken GetUserTokenByUserName(string UserName)
        {
            if (AccountsLibrary.TryGetValue(UserName, out UserToken accID))
                return accID;
            throw new KeyNotFoundException($"The UserName {UserName} does not exist in nio2so.");
        }
    }
}

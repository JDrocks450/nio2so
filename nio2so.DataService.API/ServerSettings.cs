using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API
{
    /// <summary>
    /// Settings for the nio2so Database Service Server
    /// </summary>
    internal class ServerSettings
    {
        public static ServerSettings Current { get; } = new ServerSettings();


        public string DatabaseDirectory { get; set; } = TestingConstraints.DatabaseDirectory;
        public string AccountsFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\accounts.json";
        public string UsersFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\users.json";
        public string AvatarInfoFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\avatarinfo.json";

        /// <summary>
        /// Accounts that are always present in the <see cref="AccountsFile"/>
        /// </summary>
        public UserToken[] StaticAccounts { get; } =
        [
            "bloaty@0001", "friendly@0001"
        ];
    }
}

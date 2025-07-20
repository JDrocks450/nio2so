using nio2so.Data.Common.Testing;

namespace nio2so.DataService.API
{
    internal class ServerSettings
    {
        public static ServerSettings Current { get; } = new ServerSettings();
        public string DatabaseDirectory { get; set; } = TestingConstraints.DatabaseDirectory;
        public string AccountsFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\accounts.json";
        public string UsersFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\users.json";
    }
}

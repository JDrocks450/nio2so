using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Lot;
using System.Text.Json;

namespace nio2so.DataService.API
{
    /// <summary>
    /// Settings for the nio2so Database Service Server
    /// </summary>
    internal class ServerSettings
    {
        public static ServerSettings Current { get; } = new ServerSettings();

        public JsonSerializerOptions SerializationOptions { get; set; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public string WorkspaceDirectory { get; set; } = TestingConstraints.WorkspaceDirectory;
        public string DatabaseDirectory { get; set; } = TestingConstraints.DatabaseDirectory;
        public string AccountsFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\accounts.json";
        public string UsersFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\users.json";
        public string AvatarInfoFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\avatarinfo.json";
        public string LotInfoFile { get; set; } = TestingConstraints.DatabaseDirectory + @"\lotinfo.json";

        public string ConstantsDirectory { get; set; } = Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const");

        /// <summary>
        /// Accounts that are always present in the <see cref="AccountsFile"/>
        /// </summary>
        public UserToken[] StaticAccounts { get; } =
        [
            "bloaty@0001", "friendly@0001"
        ];
        /// <summary>
        /// Lots that are always present in the <see cref="LotInfoFile"/>
        /// </summary>
        public LotProfile[] StaticLots { get; } =
        {
            new(TestingConstraints.MyHouseID, TestingConstraints.MyAvatarID, new(93, 135), TestingConstraints.MyHousePhoneNumber,
                TestingConstraints.MyHouseName, "Welcome to the castle.")
        };

        public string DefaultCharblobPath { get; set; } = Path.Combine(Current?.ConstantsDirectory ?? Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const"), "default_charblob.charblob");
        public string DefaultThumbnailPath { get; set; } = Path.Combine(Current?.ConstantsDirectory ?? Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const"), "default_thumbnail.png");

        public uint LotPurchasePrice { get; } = 2400;
    }
}

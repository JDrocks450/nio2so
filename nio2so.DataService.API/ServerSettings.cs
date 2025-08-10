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

        public string ConstantsDirectory { get; set; } = Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const");
        public string WorkspaceDirectory { get; set; } = TestingConstraints.WorkspaceDirectory;
        public string DatabaseDirectory { get; set; } = TestingConstraints.DatabaseDirectory;
        public string AccountsFile => DatabaseDirectory + @"\accounts.json";
        public string UsersFile => DatabaseDirectory + @"\users.json";
        public string AvatarInfoFile => DatabaseDirectory + @"\avatarinfo.json";
        public string LotInfoFile => DatabaseDirectory + @"\lotinfo.json";
        public string LotCreationIndexFile => DatabaseDirectory + @"\LCID.json";
        public string AvatarCreationIndexFile => DatabaseDirectory + @"\ACID.json";
        public string HouseBlobLibraryDirectory => Path.Combine(DatabaseDirectory, "houseblob");
        public string AvatarBlobLibraryPath => Path.Combine(DatabaseDirectory, "charblob");

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
            new(TestingConstraints.MyHouseLotID, TestingConstraints.MyAvatarID, new(93, 135), TestingConstraints.MyHouseName, "Welcome to the castle.")
        };

        public string DefaultCharblobPath { get; set; } = Path.Combine(Current?.ConstantsDirectory ?? Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const"), "default_charblob.charblob");
        public string DefaultThumbnailPath { get; set; } = Path.Combine(Current?.ConstantsDirectory ?? Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const"), "default_thumbnail.png");        
        public string DefaultHouseBlobPath { get; set; } = Path.Combine(Current?.ConstantsDirectory ?? Path.Combine(Current?.WorkspaceDirectory ?? TestingConstraints.WorkspaceDirectory, "const"), "default_houseblob.houseblob");
        
        public int LotPurchasePrice { get; } = 2400;        
    }
}

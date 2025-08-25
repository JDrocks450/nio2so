using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Lot;
using System.Text.Json;

namespace nio2so.DataService.Common.Types
{
    /// <summary>
    /// Settings for the nio2so Database Service Server
    /// </summary>
    public class ServerSettings
    {
        public const string ConstantsDirectoryToken = "{ConstantsDirectory}";
        public const string WorkspaceDirectoryToken = "{WorkspaceDirectory}";
        public const string DatabaseDirectoryToken = "{DatabaseDirectory}";

        /// <summary>
        /// The default settings for a <see cref="ServerSettings"/> instance
        /// </summary>
        public static ServerSettings Default => new();

        /// <summary>
        /// Settings that are downloaded by the Voltron Server when it is being initialized
        /// </summary>
        public VoltronServerSettings VoltronSettings { get; set; } = new();

        public JsonSerializerOptions SerializationOptions { get; set; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IndentCharacter = '\t',
        };

        public string WorkspaceDirectory { get; set; } = TestingConstraints.WorkspaceDirectory;
        public string ConstantsDirectory { get; set; } = Path.Combine(WorkspaceDirectoryToken, "const");        
        public string DatabaseDirectory { get; set; } = Path.Combine(WorkspaceDirectoryToken,"db");
        public string AccountsFile => DatabaseDirectoryToken + @"\accounts.json";
        public string UsersFile => DatabaseDirectoryToken + @"\users.json";
        public string AvatarInfoFile => DatabaseDirectoryToken + @"\avatarinfo.json";
        public string LotInfoFile => DatabaseDirectoryToken + @"\lotinfo.json";
        public string LotCreationIndexFile => DatabaseDirectoryToken + @"\LCID.json";
        public string AvatarCreationIndexFile => DatabaseDirectoryToken + @"\ACID.json";
        public string HouseBlobLibraryDirectory => Path.Combine(DatabaseDirectoryToken, "houseblob");
        public string AvatarBlobLibraryPath => Path.Combine(DatabaseDirectoryToken, "charblob");
        public string InboxServiceFile => DatabaseDirectoryToken + @"\inboxmessages.json";

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

        public string DefaultCharblobPath { get; set; } = Path.Combine(ConstantsDirectoryToken, "default_charblob.charblob");
        public string DefaultThumbnailPath { get; set; } = Path.Combine(ConstantsDirectoryToken, "default_thumbnail.png");        
        public string DefaultHouseBlobPath { get; set; } = Path.Combine(ConstantsDirectoryToken, "default_houseblob.houseblob");
        
        public int LotPurchasePrice { get; } = 2400;
        /// <summary>
        /// Should walk the user through setting up settings for their server when flag is set
        /// </summary>
        public bool FirstRunExperience { get; set; } = true;
        
        public string DereferencePath(string SpecialPath)
        {
            string[] tokens = SpecialPath.Split('\\');
            bool found = true;
            while (found)
            {
                found = false;
                for (int i = 0; i < tokens.Length; i++)
                {
                    string before = tokens[i];
                    string value = before.Replace(ConstantsDirectoryToken, ConstantsDirectory)
                                 .Replace(WorkspaceDirectoryToken, WorkspaceDirectory)
                                 .Replace(DatabaseDirectoryToken, DatabaseDirectory);
                    if (value != before)
                        found = true;
                    tokens[i] = value;
                }
            }
            return Path.Combine(tokens);
        }
    }
}

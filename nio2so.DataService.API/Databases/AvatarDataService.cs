using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Avatar information:
    /// <list type="bullet">Profile information of the Avatar shown in SAS</list>
    /// </summary>
    internal class AvatarDataService : DatabaseComponentBase<DataComponentDictionaryDataFile<AvatarIDToken, AvatarInfo>>
    {
        internal AvatarDataService() : base(ServerSettings.Current.AvatarInfoFile)
        {
            CreateDefaultValues();
        }

        protected override void CreateDefaultValues()
        {
            ServerSettings settings = ServerSettings.Current;
            DataFile.TryAdd(1337, new AvatarInfo()
            {
                AccountOwner = settings.StaticAccounts[0],
                Profile = new AvatarProfile(1337, "bisquick", 0, 0, 0, 0, "Blazing Falls")
            });
            // not needed
        }       

        /// <summary>
        /// Returns the <see cref="AvatarProfile"/> for the given <paramref name="AvatarID"/>. The profile has no personal details and requires no validation **public info**
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public AvatarProfile GetProfileByAvatarID(AvatarIDToken AvatarID)
        {
            void correctErrors(AvatarProfile Profile)
            {
                // TODO
            }
            if (!DataFile.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            AvatarProfile profile = DataFile[AvatarID].Profile;
            correctErrors(profile);
            return profile;
        }
    }
}

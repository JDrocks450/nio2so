using nio2so.DataService.API.Databases;

namespace nio2so.DataService.API
{
    /// <summary>
    /// <code>Temporary solution ... subject to change to reflection strategy</code>
    /// </summary>
    internal static class APIDataServices
    {
        internal static AvatarDataService AvatarDataService { get; } = new AvatarDataService();
        internal static AccountsDataService AccountService { get; } = new AccountsDataService();
        internal static UserDataService UserDataService { get; } = new UserDataService();
        internal static LotsDataService LotDataService { get; } = new LotsDataService();
    }
}

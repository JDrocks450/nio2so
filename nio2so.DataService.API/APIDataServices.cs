using nio2so.DataService.API.Databases;

namespace nio2so.DataService.API
{
    internal static class APIDataServices
    {
        public static AvatarDataService AvatarDataService { get; } = new AvatarDataService();
        internal static AccountsDataService AccountService { get; } = new AccountsDataService();
        internal static UserDataService UserDataService { get; } = new UserDataService();
    }
}

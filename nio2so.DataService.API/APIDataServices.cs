using nio2so.DataService.API.Databases;

namespace nio2so.DataService.API
{
    /// <summary>
    /// <code>Temporary solution ... subject to change to reflection strategy</code>
    /// </summary>
    internal static class APIDataServices
    {
        internal static ConfigurationDataService ConfigService { get; } = new ConfigurationDataService(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "C:\\nio2so", "settings.json"));
        internal static AvatarDataService AvatarDataService { get; } = new AvatarDataService();
        internal static AccountsDataService AccountService { get; } = new AccountsDataService();
        internal static UserDataService UserDataService { get; } = new UserDataService();
        internal static LotsDataService LotDataService { get; } = new LotsDataService();        

        private static Dictionary<Type, DataServiceBase> _dataServices = new()
        {
            { typeof(AvatarDataService), AvatarDataService },
            { typeof(AccountsDataService), AccountService },
            { typeof(UserDataService), UserDataService },            
            { typeof(LotsDataService), LotDataService },
            { typeof(ConfigurationDataService), ConfigService },
        };

        /// <summary>
        /// Gets the global instance of the specified <see cref="DataServiceBase"/> <typeparamref name="T"/> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T GetDataService<T>() where T : DataServiceBase =>
            (T)_dataServices[typeof(T)];
    }
}

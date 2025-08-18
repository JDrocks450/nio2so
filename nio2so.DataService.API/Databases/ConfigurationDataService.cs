using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Types;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Handles server configuration settings requests to view/modify the <see cref="ServerSettings"/> for this data service
    /// </summary>
    internal class ConfigurationDataService : DataServiceBase
    {
        public bool IsReady { get; }
        private JSONObjectLibrary<ServerSettings> settings => GetLibrary<JSONObjectLibrary<ServerSettings>>("SETTINGS");
        public string SettingsPath { get; }

        public ConfigurationDataService(string SettingsPath) : base()
        {
            this.SettingsPath = SettingsPath;
            
            //**add serialized settings
            var lib = new JSONObjectLibrary<ServerSettings>(SettingsPath, EnsureDefaultValues);
            Libraries.Add("SETTINGS", lib);
            IsReady = true; 
            lib.InvokeEnsureDefaultValues().Wait();            
        }

        protected override void AddLibraries()
        {
            base.AddLibraries();
        }

        Task EnsureDefaultValues()
        {            
            settings.DataFile = settings.DataFile ?? new ServerSettings();
            return settings.Save();
        }

        /// <summary>
        /// Gets the <see cref="ServerSettings"/> instance stored in this <see cref="ConfigurationDataService"/>
        /// </summary>
        /// <returns></returns>
        public ServerSettings GetCurrentSettings() => settings.DataFile;
        /// <summary>
        /// Sets the <see cref="ServerSettings"/> instance stored in this <see cref="ConfigurationDataService"/>
        /// </summary>
        /// <returns></returns>
        public Task SetCurrentSettings(ServerSettings Settings)
        {
            settings.DataFile = Settings;
            return settings.Save();
        }
    }
}

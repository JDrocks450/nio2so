using nio2so.DataService.API.Databases.Libraries;

namespace nio2so.DataService.API.Databases
{

    public abstract class DataServiceBase
    {
        protected Dictionary<string, IDataServiceLibrary> Libraries { get; } = new();
        /// <summary>
        /// Gets the current <see cref="ServerSettings"/> instance from the <see cref="ConfigurationDataService"/> global instance in <see cref="APIDataServices"/>
        /// </summary>
        protected ServerSettings CurrentSettings => APIDataServices.ConfigService.GetCurrentSettings();

        protected DataServiceBase()
        {
            AddLibraries();
        }

        protected virtual void AddLibraries()
        {
            foreach (var library in Libraries)
                library.Value.InvokeEnsureDefaultValues();
        }

        protected T GetLibrary<T>(string Name) where T : class, IDataServiceLibrary => (T)Libraries[Name];

        protected async Task Save()
        {
            foreach (var library in Libraries)
                await library.Value.Save();
        }
    }
}

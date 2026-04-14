using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Types;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// A basic data service component that stores a collection of <see cref="IDataServiceLibrary"/> for storing, retrieving and organizing data.
    /// <para/>Data Services make up the core Information Input Output system for the nio2so data service, they store a collection of <see cref="IDataServiceLibrary"/>
    /// instances that are written/read to/from the disk. They serialize data in different formats depending on which Library type you use.
    /// </summary>
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

        /// <summary>
        /// This function is called when the data service is initialized. Inheritors should add all of their libraries to <see cref="Libraries"/>
        /// programmatically that they intend to use for this Data Service's normal functions.
        /// <para/>Add all libraries, and ensure you call <c>base.AddLibraries();</c> afterward to then invoke <see cref="IDataServiceLibrary.InvokeEnsureDefaultValues"/>
        /// </summary>
        protected virtual void AddLibraries()
        {
            foreach (var library in Libraries)
                library.Value.InvokeEnsureDefaultValues();
        }
        /// <summary>
        /// Gets a <see cref="IDataServiceLibrary"/> added to <see cref="Libraries"/> by its <paramref name="Name"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Name"></param>
        /// <returns></returns>
        protected T GetLibrary<T>(string Name) where T : class, IDataServiceLibrary => (T)Libraries[Name];
        /// <summary>
        /// Saves all appended <see cref="Libraries"/> to the disk
        /// <para/>This should be used as opposed to iterating over each Library and saving individually.
        /// </summary>
        /// <returns></returns>
        protected async Task Save()
        {
            foreach (var library in Libraries)
            {
                try
                {
                    await library.Value.Save();
                }
                catch (Exception ex)
                {
                    // add rollback tech here
                    throw ex;
                }
            }
        }
    }
}

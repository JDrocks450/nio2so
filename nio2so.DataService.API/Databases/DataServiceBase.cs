using nio2so.DataService.API.Databases.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.API.Databases
{

    internal abstract class DataServiceBase
    {
        protected Dictionary<string, IDataServiceLibrary> Libraries { get; } = new();

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

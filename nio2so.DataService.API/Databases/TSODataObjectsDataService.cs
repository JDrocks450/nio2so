using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Top100;
using nio2so.Formats.TSOData;
using System.Dynamic;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves <see cref="TSODataRuntimeObject"/>s for Objects stored in the TSO Data Service format (using the nio2so Data Service as a vehicle for the data)
    /// </summary>
    public class TSODataObjectsDataService : DataServiceBase
    {
        [Serializable]
        public class TSODataObjectStoredValue
        {
            public TSODataObjectStoredValue(uint TSOData_DataStructID, uint ObjectID, object TSODataObject)
            {
                this.TSOData_DataStructID = TSOData_DataStructID;
                this.ObjectID = ObjectID;
                this.TSODataObject = TSODataObject;
            }
            public object TSODataObject { get; set; } = null;
            public uint TSOData_DataStructID { get; }
            public uint ObjectID { get; }
        }

        const string SERVICE_NAME = "TSOData_DataService";

        string TSODataWorkingDirectory;

        public TSODataObjectsDataService() : base()
        {

        }

        protected override void AddLibraries()
        {
            ServerSettings settings = CurrentSettings;

            TSODataWorkingDirectory = settings.DereferencePath(settings.TSOData_DataServiceDirectoryPath);            

            base.AddLibraries();
        }

        /// <summary>
        /// Gets (or Loads if unloaded) (or Creates if not existing) an object library for the given <paramref name="TSOData_StructName"/>
        /// </summary>
        /// <param name="TSOData_StructName"></param>
        private JSONDictionaryLibrary<uint, TSODataObjectStoredValue> GetOrCreateLibrary(string TSOData_StructName)
        {
            if (!Libraries.TryGetValue(TSOData_StructName, out IDataServiceLibrary? library)) {
                string FilePath = Path.Combine(TSODataWorkingDirectory, $"{TSOData_StructName}.json");
                Libraries.Add(TSOData_StructName, new JSONDictionaryLibrary<uint, TSODataObjectStoredValue>(FilePath, EnsureDefaultLists));
            }
            return (JSONDictionaryLibrary<uint, TSODataObjectStoredValue>)Libraries[TSOData_StructName];
        }

        public bool TryGetTSODataObject(string TSOData_StructName, uint ObjectID, out TSODataObjectStoredValue Value)
        {
            JSONDictionaryLibrary<uint, TSODataObjectStoredValue> lib = GetOrCreateLibrary(TSOData_StructName);
            bool found = lib.TryGetValue(ObjectID, out Value);
            if (found)
            {
                if (Value.TSODataObject is JsonElement)
                    Value.TSODataObject = ((JsonElement)Value.TSODataObject).Deserialize<ExpandoObject>();
            }
            return found;
        }
        public void TrySetTSODataObject(uint ObjectID, TSODataRuntimeObject Value)
        {
            JSONDictionaryLibrary<uint, TSODataObjectStoredValue> lib = GetOrCreateLibrary(Value.Structure.NameString);
            TSODataObjectStoredValue newValue = new(Value.Structure.NameID, ObjectID, Value.Instance);
            bool returnValue = false;
            if (!lib.TryGetValue(ObjectID, out var storedValue))
                returnValue = lib.TryAdd(ObjectID, newValue);
            else
            {
                if (storedValue.TSOData_DataStructID != Value.Structure.NameID)
                    throw new InvalidDataException("DataStruct ID mis-match.");
                storedValue.TSODataObject = Value.Instance;
                returnValue = true;
            }
            if (returnValue)
                Save().Wait();            
        }                

        /// <summary>
        /// Does nothing, currently.
        /// </summary>
        /// <returns></returns>
        Task EnsureDefaultLists() => Task.CompletedTask; // these are populated from the TSODataDefinition as needed.

        
    }
}

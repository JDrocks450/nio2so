using System.Text.Json;

namespace nio2so.DataService.API.Databases.Libraries
{
    internal class JSONObjectLibrary<T> : IDataServiceLibrary where T : class, new()
    {
        private string _baseDir;
        public string? BaseDirectory => Path.GetDirectoryName(_baseDir);

        public T DataFile { get; set; }

        private Action CreateDefaultValues;

        /// <summary>
        /// Creates a new <see cref="DatabaseComponentBase{T1, T2}"/> with the given home directory
        /// </summary>
        /// <param name="FilePath"></param>
        public JSONObjectLibrary(string FilePath, Action EnsureDefaultValuesFunc, bool DelayedLoad = false)
        {
            _baseDir = FilePath;
            DataFile = new();
            CreateDefaultValues = EnsureDefaultValuesFunc;
            if (DelayedLoad) return;
            Load().Wait();
        }

        public async Task Load()
        {
            DataFile = await LoadDataFile<T>(_baseDir) ?? DataFile;            
        }

        public void InvokeEnsureDefaultValues() => CreateDefaultValues();

        public static async Task<T?> LoadDataFile<T>(string DBPath)
        {
            if (!File.Exists(DBPath)) return default;
            using FileStream fs = File.OpenRead(DBPath);
            return await JsonSerializer.DeserializeAsync<T>(fs, ServerSettings.Current.SerializationOptions);
        }

        public Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_baseDir));
            using FileStream fs = File.Create(_baseDir);
            lock (DataFile)
            {
                return JsonSerializer.SerializeAsync(fs, DataFile);
            }
        }
    }
}

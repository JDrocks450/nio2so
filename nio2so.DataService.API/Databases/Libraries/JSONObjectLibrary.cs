using System.Text.Json;

namespace nio2so.DataService.API.Databases.Libraries
{
    internal class JSONObjectLibrary<T> : IDataServiceLibrary where T : class, new()
    {
        private string _baseDir;
        public string? BaseDirectory => Path.GetDirectoryName(_baseDir);

        public T DataFile { get; set; }

        private Func<Task> CreateDefaultValues;

        /// <summary>
        /// Creates a new <see cref="DatabaseComponentBase{T1, T2}"/> with the given home directory
        /// </summary>
        /// <param name="FilePath"></param>
        public JSONObjectLibrary(string FilePath, Func<Task>? EnsureDefaultValuesFunc = default, bool DelayedLoad = false)
        {
            _baseDir = FilePath;
            if (FilePath.Contains('{'))
                throw new Exception(FilePath + " is invalid.");
            DataFile = new();
            CreateDefaultValues = EnsureDefaultValuesFunc ?? (() => Task.CompletedTask);
            if (DelayedLoad) return;            
            Load().Wait();
        }

        public async Task Load()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IndentCharacter = '\t',                
            };
            if (APIDataServices.ConfigService?.IsReady ?? false)
                options = APIDataServices.ConfigService.GetCurrentSettings().SerializationOptions;
            DataFile = await LoadDataFile<T>(_baseDir) ?? DataFile;            
        }

        public Task InvokeEnsureDefaultValues() => CreateDefaultValues();

        public static async Task<T?> LoadDataFile<T>(string DBPath, JsonSerializerOptions? Options = default)
        {
            if (!File.Exists(DBPath)) return default;
            using FileStream fs = File.OpenRead(DBPath);
            return await JsonSerializer.DeserializeAsync<T>(fs, Options);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace nio2so.DataService.API.Databases
{
    internal abstract class DatabaseComponentBase<T> where T : IDatabaseComponentDataFile, new()
    {
        private string _baseDir;
        protected string BaseDirectory => Path.GetDirectoryName(_baseDir);

        protected T DataFile { get; set; }

        /// <summary>
        /// Creates a new <see cref="DatabaseComponentBase{T1, T2}"/> with the given home directory
        /// </summary>
        /// <param name="FilePath"></param>
        protected DatabaseComponentBase(string FilePath, bool DelayedLoad = false)
        {
            _baseDir = FilePath;
            DataFile = new();
            if (DelayedLoad) return;
            Load().Wait();
        }

        public async Task Load()
        {
            DataFile = await LoadDataFile<T>(_baseDir) ?? DataFile;
            if (!DataFile.HasDefaultValues()) CreateDefaultValues();
        }

        public static async Task<T?> LoadDataFile<T>(string DBPath)
        {
            if (!File.Exists(DBPath)) return default;
            using FileStream fs = File.OpenRead(DBPath);
            return await JsonSerializer.DeserializeAsync<T>(fs);
        }

        public Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_baseDir));
            using FileStream fs = File.Create(_baseDir);
            lock (DataFile)
            {
                return JsonSerializer.SerializeAsync<T>(fs, DataFile);
            }
        }

        protected abstract void CreateDefaultValues();
    }
}

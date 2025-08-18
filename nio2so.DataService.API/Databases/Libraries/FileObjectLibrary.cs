namespace nio2so.DataService.API.Databases.Libraries
{
    /// <summary>
    /// A library serving files in a directory
    /// </summary>
    public class FileObjectLibrary : IDataServiceLibrary
    {
        /// <summary>
        /// Gets the directory to write files to the disk at
        /// </summary>
        public string BaseDirectory { get; }
        /// <summary>
        /// Gets the name of the type of file to add to the disk
        /// <para>Example: "house" will save "house[HouseID].[<see cref="Extension"/>]</para>
        /// </summary>
        public string ItemName { get; }
        /// <summary>
        /// Gets the extension of the files to write to the disk
        /// </summary>
        public string Extension { get; }

        private Func<Task<byte[]>> OnFileNotFound;

        public FileObjectLibrary(string baseDirectory, string itemName, string extension, Func<Task<byte[]>> onFileNotFound)
        {
            BaseDirectory = baseDirectory;
            if (baseDirectory.Contains('{'))
                throw new Exception(baseDirectory + " is invalid.");
            ItemName = itemName;
            Extension = extension;
            OnFileNotFound = onFileNotFound;
        }

        public virtual string GetObjectURI(uint ObjectID, string? Extension = default)
        {
            string HouseDirectory = BaseDirectory;
            string ext = Extension ?? this.Extension;
            if (!ext.StartsWith("."))
                ext = "." + ext;
            string HouseFileName = $"{ItemName}{ObjectID}{ext}";
            return Path.Combine(HouseDirectory, HouseFileName);
        }

        /// <summary>
        /// Uses the <see cref="TSOVoltronSerializer"/> to deserialize the data stored on the disk at the given
        /// <paramref name="ObjectID"/> and extension provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectID"></param>
        /// <param name="OverrideExtension"></param>
        /// <returns></returns>
        //protected T GetDataObjectByID<T>(uint ObjectID, string? OverrideExtension = default) where T : new() =>
          //  TSOVoltronSerializer.Deserialize<T>(GetDataByID(ObjectID, OverrideExtension));

        /// <summary>
        /// Returns a <see langword="byte"/> array containing the file data at the <see cref="MY_DIR"/> with the given
        /// extension and ObjectID.
        /// </summary>
        /// <param name="ObjectID"></param>
        /// <param name="OverrideExtension"></param>
        /// <returns></returns>
        public async Task<byte[]> GetDataByID(uint ObjectID, string? OverrideExtension = default)
        {
            string uri = GetObjectURI(ObjectID, OverrideExtension);
            if (!File.Exists(uri))
            {
                //TSOLoggerServiceBase.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message,
                //GetType().Name, $"Get {MY_ITEMNAME} ID: {ObjectID} not found. Sending default value if available..."));
                return await OnFileNotFound();
            }
            byte[] buffer = await File.ReadAllBytesAsync(uri);
            //TSOLoggerServiceBase.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message,
              //  GetType().Name, $"Get {MY_ITEMNAME} ID: {ObjectID} success! Size: {buffer.Length}"));
            return buffer;
        }

        /// <summary>
        /// Sets the given <paramref name="ObjectData"/> to the disk by the <paramref name="ObjectID"/> provided.
        /// <para/>Uses the <see cref="TSOVoltronSerializer"/> to write bytes to the disk.
        /// <para/>If you notice issues with serialization, please refer to <see cref="TSOVoltronSerializerCore"/>
        /// to see where issues may be arising with your data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectID">The ID of the <paramref name="ObjectData"/> to set</param>
        /// <param name="ObjectData">The Object which you want to save to the disk</param>
        /// <param name="Overwrite">Can we overwrite pre-existing data?</param>
        /// <param name="OverrideExtension">What extension you would like to save it with. Default: <see cref="Extension"/></param>
        //protected void SetDataObjectByIDToDisk<T>(uint ObjectID, T ObjectData, bool Overwrite = true, string? OverrideExtension = default) where T : new() =>
          //  SetDataByIDToDisk(ObjectID, TSOVoltronSerializer.Serialize(ObjectData), Overwrite, OverrideExtension);

        /// <summary>
        /// Writes the <see cref="TSODBHouseBlob"/> to the disk at <see cref="HOUSE_DIR"/>
        /// </summary>
        /// <param name="ObjectID"></param>
        /// <param name="houseBlob"></param>
        public Task SetDataByIDToDisk(uint ObjectID, byte[] Buffer, bool Overwrite = true, string? OverrideExtension = default)
        {
            Directory.CreateDirectory(BaseDirectory);
            return File.WriteAllBytesAsync(GetObjectURI(ObjectID, OverrideExtension), Buffer);
            //TSOLoggerServiceBase.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message,
             //   GetType().Name, $"Set {MY_ITEMNAME} ID: {ObjectID} successfully. Size: {Buffer.Length} (Can Overwrite: {Overwrite})"));
        }

        Task IDataServiceLibrary.Load() { return Task.CompletedTask; }

        Task IDataServiceLibrary.Save() { return Task.CompletedTask; }

        async Task IDataServiceLibrary.InvokeEnsureDefaultValues() { }
    }
}

using nio2so.Formats.DB;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nio2so.TSOTCP.Voltron.Protocol.Factory
{
    /// <summary>
    /// Marks this type as a <see cref="TSOFactoryBase"/> to be added to the Type map
    /// </summary>
    public class TSOFactoryAttribute : Attribute
    {
        public TSOFactoryAttribute() : base() { }
    }

    public interface ITSOFactory
    {
        //**IMPLEMENT LATER ONCE DECIDING ON A COMMON DATA FORMAT FOR FACTORIES
    }

    public abstract class TSOFactoryBase : ITSOFactory
    {
        private static Dictionary<Type, TSOFactoryBase> _factories = new();

        /// <summary>
        /// Gets the directory to write files to the disk at
        /// </summary>
        protected abstract string MY_DIR { get; }
        /// <summary>
        /// Gets the name of the type of file to add to the disk
        /// <para>Example: "house" will save "house[HouseID].[<see cref="MY_EXT"/>]</para>
        /// </summary>
        protected abstract string MY_ITEMNAME { get; }
        /// <summary>
        /// Gets the extension of the files to write to the disk
        /// </summary>
        protected abstract string MY_EXT { get; }

        /// <summary>
        /// Gets the <see cref="TSOFactoryBase"/> by type
        /// <para>Throws exception if requested <see cref="TSOFactoryBase"/> is not found.</para>
        /// <para>Use <see cref="Register"/> to register a new Factory</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : TSOFactoryBase
        {
            if (typeof(T) == typeof(TSOFactoryBase))
                throw new InvalidOperationException("Every factory is TSOFactoryBase, give a type that's more specific.");
            return (T)_factories[typeof(T)];
        }
        /// <summary>
        /// Registers <see langword="this"/> <see cref="ITSOFactory"/> to the type map
        /// </summary>
        /// <returns></returns>
        protected bool Register()
        {
            bool result = _factories.TryAdd(GetType(), this);
            if (result)
                TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                    GetType().Name, "Registered to the cTSOFactory type map"));
            return result;
        }

        /// <summary>
        /// Calls <see cref="InitializeFactories"/> <para/>
        /// Uses reflection to find all TSOFactories with the <see cref="TSOFactoryAttribute"/>
        /// </summary>
        static TSOFactoryBase()
        {
            InitializeFactories();
        }

        /// <summary>
        /// Uses reflection to find all TSOFactories with the <see cref="TSOFactoryAttribute"/>
        /// </summary>
        public static void InitializeFactories()
        {
            var assembly = typeof(TSOFactoryBase).Assembly;
            //USE REFLECTION TO MAP TYPES
            foreach (var type in assembly.GetTypes().
                Where(x => x.GetCustomAttribute<TSOFactoryAttribute>() != default))
            {
                //Has property and is the correct type ... add this!
                ((TSOFactoryBase)assembly.CreateInstance(type.FullName)).Register();
            }
        }

        /// <summary>
        /// Registers this <see cref="ITSOFactory"/> to the type map
        /// </summary>
        protected TSOFactoryBase()
        {
            Register();
        }

        protected abstract byte[] OnFileNotFound();

        public virtual string GetObjectURI(uint ObjectID, string? Extension = default)
        {
            string HouseDirectory = MY_DIR;
            string ext = Extension ?? MY_EXT;
            if (!ext.StartsWith("."))
                ext = "." + ext;
            string HouseFileName = $"{MY_ITEMNAME}{ObjectID}{ext}";
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
        protected T GetDataObjectByID<T>(uint ObjectID, string? OverrideExtension = default) where T : new() =>
            TSOVoltronSerializer.Deserialize<T>(GetDataByID(ObjectID, OverrideExtension));

        /// <summary>
        /// Returns a <see langword="byte"/> array containing the file data at the <see cref="MY_DIR"/> with the given
        /// extension and ObjectID.
        /// </summary>
        /// <param name="ObjectID"></param>
        /// <param name="OverrideExtension"></param>
        /// <returns></returns>
        protected byte[] GetDataByID(uint ObjectID, string? OverrideExtension = default)
        {
            string uri = GetObjectURI(ObjectID, OverrideExtension);
            if (!File.Exists(uri))
            {
                TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                GetType().Name, $"Get {MY_ITEMNAME} ID: {ObjectID} not found. Sending default value if available..."));
                return OnFileNotFound();
            }
            byte[] buffer = File.ReadAllBytes(uri);
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                GetType().Name, $"Get {MY_ITEMNAME} ID: {ObjectID} success! Size: {buffer.Length}"));
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
        /// <param name="OverrideExtension">What extension you would like to save it with. Default: <see cref="MY_EXT"/></param>
        protected void SetDataObjectByIDToDisk<T>(uint ObjectID, T ObjectData, bool Overwrite = true, string? OverrideExtension = default) where T : new() =>
            SetDataByIDToDisk(ObjectID, TSOVoltronSerializer.Serialize(ObjectData), Overwrite, OverrideExtension);

        /// <summary>
        /// Writes the <see cref="TSODBHouseBlob"/> to the disk at <see cref="HOUSE_DIR"/>
        /// </summary>
        /// <param name="ObjectID"></param>
        /// <param name="houseBlob"></param>
        protected void SetDataByIDToDisk(uint ObjectID, byte[] Buffer, bool Overwrite = true, string? OverrideExtension = default)
        {
            Directory.CreateDirectory(MY_DIR);
            File.WriteAllBytes(GetObjectURI(ObjectID, OverrideExtension), Buffer);
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                GetType().Name, $"Set {MY_ITEMNAME} ID: {ObjectID} successfully. Size: {Buffer.Length} (Can Overwrite: {Overwrite})"));
        }
#if DEBUG
        public void Debug_SetCustomDataToDisk(uint DebugObjectID, string DataTypeName, byte[] WriteBytes, bool Overwrite = true) =>
            SetDataByIDToDisk(DebugObjectID, WriteBytes, Overwrite, DataTypeName);
        public void Debug_SetCustomDataToDisk<T>(uint DebugObjectID, string DataTypeName, T WriteObject, bool Overwrite = true)
            where T : new() => SetDataObjectByIDToDisk(DebugObjectID, WriteObject, Overwrite, DataTypeName);
        public byte[] Debug_GetDataByID(uint ObjectID, string DataTypeName) => GetDataByID(ObjectID, DataTypeName);
        public T Debug_GetDataByID<T>(uint ObjectID, string DataTypeName) where T : new() => GetDataObjectByID<T>(ObjectID, DataTypeName);
#endif
    }
}

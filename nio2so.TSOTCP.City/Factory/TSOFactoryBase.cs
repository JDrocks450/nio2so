using nio2so.Formats.DB;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nio2so.TSOTCP.City.Factory
{
    /// <summary>
    /// Marks this type as a <see cref="TSOFactoryBase"/> to be added to the Type map
    /// </summary>
    public class TSOFactoryAttribute : Attribute
    {
        public TSOFactoryAttribute() : base() { }    
    }

    internal interface ITSOFactory
    {
        //**IMPLEMENT LATER ONCE DECIDING ON A COMMON DATA FORMAT FOR FACTORIES
    }

    internal abstract class TSOFactoryBase : ITSOFactory
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
                TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
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

        protected byte[] GetDataByID(uint ObjectID, string? OverrideExtension = default)
        {
            string uri = GetObjectURI(ObjectID,OverrideExtension);
            if (!File.Exists(uri))
            {
                TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                GetType().Name, $"Get {MY_ITEMNAME} ID: {ObjectID} not found. Sending default value if available..."));
                return OnFileNotFound();
            }
            byte[] buffer = File.ReadAllBytes(uri);
            TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                GetType().Name, $"Get {MY_ITEMNAME} ID: {ObjectID} success! Size: {buffer.Length}"));
            return buffer;
        }

        /// <summary>
        /// Writes the <see cref="TSODBHouseBlob"/> to the disk at <see cref="HOUSE_DIR"/>
        /// </summary>
        /// <param name="ObjectID"></param>
        /// <param name="houseBlob"></param>
        protected void SetDataByIDToDisk(uint ObjectID, byte[] Buffer, bool Overwrite = true, string? OverrideExtension = default)
        {            
            Directory.CreateDirectory(MY_DIR);
            File.WriteAllBytes(GetObjectURI(ObjectID, OverrideExtension), Buffer);
            TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                GetType().Name, $"Set {MY_ITEMNAME} ID: {ObjectID} successfully. Size: {Buffer.Length} (Can Overwrite: {Overwrite})"));
        }
    }
}

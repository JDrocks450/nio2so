using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using QuazarAPI.Networking.Data;
using System.Threading.Tasks;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.Util;
using nio2so.Data.Common.Serialization.Voltron;

namespace nio2so.TSOTCP.City.TSO.Voltron.Serialization
{
    /// <summary>
    /// Serializes and Deserializes to objects to Voltron-compatible streams
    /// </summary>
    internal static class TSOVoltronSerializer
    {                
        public static void Serialize<T>(Stream Stream, T Object)
        {
            //Does this object type implement custom serialization behavior?
            //as in-- does it define how it should deserialized itself?
            if (Object is ITSOCustomSerialize serializable)
            {
                Stream.Write(serializable.OnSerialize());
                return;
            }
            // no-- proceed with default serialize    

            //**Distance to end property attribute map is here**
            //Index, PropertyInfo
            Dictionary<uint, PropertyInfo> distanceToEnds = new();

            foreach (var property in Object.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null) 
                    continue;
                //**serializable property
                if (!TSOVoltronSerializerCore.WriteProperty(Stream, property, Object))
                    throw new ArgumentException($"Could not serialize: {property}");
            }
            //Calculate size from index of the field to the end of the file plus size of property
            foreach (var distanceToEnd in distanceToEnds)
            {
                long Size = Stream.Length - distanceToEnd.Key;
                Size -= sizeof(uint);
                distanceToEnd.Value.SetValue(Object, Size);
                Stream.SetPosition((int)distanceToEnd.Key);
                if (!TSOVoltronSerializerCore.WriteProperty(Stream, distanceToEnd.Value, Object))
                    throw new ArgumentException($"Could not serialize: {distanceToEnd.Value}");
            }
        }

        public static byte[] Serialize<T>(T Object)
        {
            using (var ms = new MemoryStream())
            {
                Serialize(ms, Object);
                return ms.ToArray();
            }
        }

        private static void doDeserialize(Stream Stream, object instance)
        {
            //Does this object type implement custom serialization behavior?
            //as in-- does it define how it should deserialized itself?
            if (instance is ITSOCustomDeserialize serializable)
            {
                serializable.OnDeserialize(Stream);
                return;
            }
            // no-- proceed with default deserialize            
            foreach (var property in instance.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null) continue;
                //**serializable property
                if (!TSOVoltronSerializerCore.ReflectProperty(Stream, property, instance))
                    throw new ArgumentException($"Could not deserialize: {property}");
                if (Stream.Position == Stream.Length) break;
            }
        }

        public static object? Deserialize(Stream Stream, Type ObjectType)
        {
            object? instance = Activator.CreateInstance(ObjectType);
            if (instance == null) return null;
            doDeserialize(Stream, instance);
            return instance;
        }
        public static T Deserialize<T>(Stream Stream) where T : new()
        {
            T newObject = new T();
            doDeserialize(Stream, newObject);
            return newObject;
        }
        public static T Deserialize<T>(byte[] streamBytes) where T : new()
        {
            using (MemoryStream stream = new MemoryStream(streamBytes))
                return Deserialize<T>(stream);
        }
        public static object Deserialize(byte[] streamBytes, Type ObjectType)
        {
            using (MemoryStream stream = new MemoryStream(streamBytes))
                return Deserialize(stream, ObjectType);
        }
    }
}

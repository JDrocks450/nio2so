using nio2so.Data.Common.Serialization.Voltron;
using QuazarAPI.Networking.Data;
using System.Reflection;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization
{
    /// <summary>
    /// Provides functionality that Serializes and Deserializes object instances to/from Voltron-compatible streams at runtime
    /// </summary>
    public static class TSOVoltronSerializer
    {
        private static Stack<TSOVoltronSerializerGraphItem> _graphStack = new();
        private static TSOVoltronSerializerGraphItem? _lastGraphItem = null;

        /// <summary>
        /// Gets all <see cref="BindingFlags.Public"/> properties that don't have <see cref="TSOVoltronIgnorable"/> or <see cref="IgnoreDataMemberAttribute"/> 
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetSerializableProperties(object Object) => 
            Object.GetType().GetProperties().Where(x => 
            x.GetCustomAttribute<IgnoreDataMemberAttribute>() == null && x.GetCustomAttribute<TSOVoltronIgnorable>() == null);

        /// <summary>
        /// <inheritdoc cref="Serialize{T}(T)"/>
        /// then writes it to <paramref name="Stream"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Stream">The <see cref="Stream"/> to write the data bytes to</param>
        /// <param name="Object">The instance of <typeparamref name="T"/> object to serialize</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Serialize<T>(Stream Stream, T Object, PropertyInfo Property = default)
        {
            void SetGraphItem(TSOVoltronSerializerGraphItem newItem)
            {
                if (!_graphStack.Any())
                {
                    _graphStack.Push(newItem);
                    return;
                }
                //_graphStack.First().Add(newItem);
                _graphStack.Push(newItem);
            }

            //Does this object type implement custom serialization behavior?
            //as in-- does it define how it should deserialized itself?
            if (Object is ITSOCustomSerialize serializable)
            {
                byte[] customData = serializable.OnSerialize();
                Stream.Write(customData);
                SetGraphItem(new(Property, Object.GetType(), Object, $"Custom serialization technique ({customData.Length} bytes)"));
                return;
            }
            // no-- proceed with default serialize    

            //**Distance to end property attribute map is here**
            //Index, PropertyInfo
            Dictionary<uint, PropertyInfo> distanceToEnds = new();

            SetGraphItem(new(Property, Object.GetType(), Object));

            foreach (var property in GetSerializableProperties(Object))
            {               
                //**serializable property
                if (!TSOVoltronSerializerCore.WriteProperty(Stream, property, Object))
                    throw new ArgumentException($"Could not serialize: {property}");
                foreach(var item in TSOVoltronSerializerCore.GetLastGraph())
                    _graphStack.First().Add(item);
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
            _lastGraphItem = _graphStack.Pop();
            
        }
        /// <summary>
        /// Serializes the given <paramref name="Object"/> to a Voltron data stream and writes it to a new <see cref="byte"/><c>[]</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Object"></param>
        /// <returns></returns>
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
            var properties = instance.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                    continue;
                //**serializable property
                if (!TSOVoltronSerializerCore.ReflectProperty(Stream, property, instance))
                    throw new ArgumentException($"Could not deserialize: {property}");
                if (Stream.Position == Stream.Length) break;
            }
        }
        /// <summary>
        /// <inheritdoc cref="Deserialize(byte[], Type)"/>
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="ObjectType"></param>
        /// <returns></returns>
        public static object? Deserialize(Stream Stream, Type ObjectType)
        {
            object? instance = Activator.CreateInstance(ObjectType);
            if (instance == null) return null;
            doDeserialize(Stream, instance);
            return instance;
        }
        /// <summary>
        /// Deserializes a <typeparamref name="T"/> from the given <paramref name="Stream"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream Stream) where T : new()
        {
            T newObject = new T();
            doDeserialize(Stream, newObject);
            return newObject;
        }
        /// <summary>
        /// Deserializes a <typeparamref name="T"/> from the <paramref name="streamBytes"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="streamBytes"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] streamBytes) where T : new()
        {
            using (MemoryStream stream = new MemoryStream(streamBytes))
                return Deserialize<T>(stream);
        }
        /// <summary>
        /// Deserializes a given <paramref name="streamBytes"/> to the destination <paramref name="ObjectType"/> 
        /// </summary>
        /// <param name="streamBytes"></param>
        /// <param name="ObjectType"></param>
        /// <returns></returns>
        public static object Deserialize(byte[] streamBytes, Type ObjectType)
        {
            using (MemoryStream stream = new MemoryStream(streamBytes))
                return Deserialize(stream, ObjectType);
        }
        /// <summary>
        /// Gets the <see cref="TSOVoltronSerializerGraphItem"/> from the last object
        /// serialized for research/studying
        /// </summary>
        /// <returns></returns>
        public static TSOVoltronSerializerGraphItem? GetLastGraph()
        {
            return _lastGraphItem;
        }
    }
}

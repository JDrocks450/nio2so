using MiscUtil.Conversion;
using System.Reflection;
using System.Text;
using QuazarAPI.Networking.Data;
using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization
{
    /// <summary>
    /// Provides low-level serialization functionality for individual data types part of a larger data contract
    /// </summary>
    public static class TSOVoltronSerializerCore
    {        
        private static T? getPropertyAttribute<T>(PropertyInfo property, out TSOVoltronValueTypes ValueType) where T : TSOVoltronValue
        {
            ValueType = TSOVoltronValueTypes.BigEndian;
            var attribute = property.GetCustomAttribute<T>();
            TSOVoltronValueTypes type = TSOVoltronValueTypes.BigEndian;
            int pascalLength = 0;
            if (attribute == null)
            {
                if (property.PropertyType == typeof(string))
                    type = TSOVoltronValueTypes.Pascal;
                else type = TSOVoltronValueTypes.BigEndian;
            }
            else
                type = attribute.Type;
            ValueType = type;
            return attribute;
        }
        /// <summary>
        /// Encodes a string to the given <paramref name="Stream"/> using the provided <paramref name="type"/> as a format specifier
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="Text"></param>
        /// <param name="type"></param>
        /// <exception cref="InvalidCastException">Unsupported string format or not a valid string format value at all</exception>
        public static void WriteString(Stream Stream, string Text, TSOVoltronValueTypes type = TSOVoltronValueTypes.Pascal)
        {
            if (type == TSOVoltronValueTypes.Pascal)
            {
                Stream.EmplaceBody(0x80, 0x00);
                Stream.EmplaceBody(EndianBitConverter.Big.GetBytes((ushort)Text.Length));
            }
            else if (type == TSOVoltronValueTypes.Length_Prefixed_Byte)
                Stream.EmplaceBody((byte)Text.Length);
            else if (type == TSOVoltronValueTypes.NullTerminated) Text += '\0';
            else throw new InvalidCastException($"{nameof(TSOVoltronSerializerCore)}::WriteString() non-exhaustive string type switch! {type} not found!");
            Stream.EmplaceBody(Encoding.UTF8.GetBytes(Text));
        }

        /// <summary>
        /// Reads an encoded string in the given <paramref name="StringType"/> format specifier from the provided <paramref name="Stream"/>
        /// </summary>
        /// <param name="StringType"></param>
        /// <param name="Stream"></param>
        /// <param name="NullTerminatedMaxLength"></param>
        /// <returns>The string read from the stream</returns>
        /// <exception cref="Exception"></exception>
        public static string ReadString(TSOVoltronValueTypes StringType, Stream Stream, int NullTerminatedMaxLength = 255)
        {
            string destValue = "Error.";
            switch (StringType)
            {
                case TSOVoltronValueTypes.Pascal:
                    {
                        ushort strHeader = Stream.ReadBodyUshort(Endianness.LittleEndian);
                        if (strHeader != 0x80)
                            throw new Exception("This is supposed to be a string but I don't think it is one...");
                        ushort len = Stream.ReadBodyUshort(Endianness.BigEndian);
                        byte[] strBytes = Stream.ReadBodyByteArray(len);
                        destValue = Encoding.UTF8.GetString(strBytes);
                    }
                    break;
                case TSOVoltronValueTypes.NullTerminated:
                    destValue = Stream.ReadBodyNullTerminatedString(NullTerminatedMaxLength);
                    break;
                case TSOVoltronValueTypes.Length_Prefixed_Byte:
                    {
                        int len = Stream.ReadBodyByte();
                        byte[] strBytes = Stream.ReadBodyByteArray(len);
                        destValue = Encoding.UTF8.GetString(strBytes);
                    }
                    break;
            }
            return destValue;
        }
        /// <summary>
        /// Deserializes the given <paramref name="property"/> from the provided <paramref name="Stream"/> and 
        /// assigns it on the <paramref name="Instance"/> provided
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="property"></param>
        /// <param name="Instance"></param>
        /// <returns>A value indicating successful deserialization</returns>
        /// <exception cref="CustomAttributeFormatException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static bool ReflectProperty(Stream Stream, PropertyInfo property, object? Instance)
        {
            Stream _bodyBuffer = Stream;
            //check if this property is the body array property
            if (property.PropertyType == typeof(byte[]))
            {
                if (property.GetCustomAttribute<TSOVoltronBodyArray>() != default)
                { // it is.
                    if (property.PropertyType != typeof(byte[]))
                        throw new CustomAttributeFormatException("You applied the TSOVoltronBodyArray attribute to ... not a byte[]! Are you testing me?");
                    int remainingData = (int)(Stream.Length - Stream.Position);
                    byte[] destBuffer = new byte[remainingData];
                    _bodyBuffer.Read(destBuffer, 0, remainingData);
                    property.SetValue(Instance, destBuffer);
                    return true; // halt execution
                }
                else throw new NotImplementedException("byte[] is the destination type to deserialize, yet you didn't " +
                    "adorn the property with the TSOVoltronBodyArrayAtrribute! This is required to ensure you understand " +
                    "that it will take EVERY byte to the end of the packet body into that property and for readability.");
            }
            //Numbers
            {
                TSOVoltronValue? attribute = getPropertyAttribute<TSOVoltronValue>(property, out TSOVoltronValueTypes type);
                bool hasAttrib = attribute != default;
                //BigEndian by default!
                Endianness dataEndianMode = type == TSOVoltronValueTypes.LittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;
                bool readValue = true;

                Type PropertyType = property.PropertyType;
                while (PropertyType.IsEnum)
                    PropertyType = Enum.GetUnderlyingType(PropertyType);

                //---NUMBERS
                if (PropertyType == typeof(byte))
                {
                    property.SetValue(Instance, (byte)Stream.ReadBodyByte());
                    return true;
                }
                else if (PropertyType == typeof(bool))
                {
                    byte value = (byte)Stream.ReadBodyByte();
                    property.SetValue(Instance, value != 0);
                    return true;
                }
                else if (PropertyType == typeof(ushort) || PropertyType == typeof(short))
                {
                    ushort fromPacket = Stream.ReadBodyUshort(dataEndianMode); // read an unsigned short
                    if (PropertyType == typeof(ushort)) // is it even an unsigned short?
                        property.SetValue(Instance, fromPacket); // yeah
                    else property.SetValue(Instance, Convert.ToInt16(fromPacket)); // no it wasn't, convert it. uhh, i think this works?
                    return true;
                }
                else if (PropertyType == typeof(uint) || PropertyType == typeof(int))
                {
                    uint fromPacket = Stream.ReadBodyDword(dataEndianMode); // read an unsigned int
                    if (PropertyType == typeof(uint)) // is it even an unsigned int?
                        property.SetValue(Instance, fromPacket); // yeah
                    else ;// property.SetValue(Instance, Convert.ToInt64(fromPacket)); // no it wasn't, convert it. uhh, i think this works?
                    return true;
                }
                else readValue = false;

                if (readValue) return true;

                if (PropertyType == typeof(DateTime))
                {
                    uint fromPacket = Stream.ReadBodyDword();
                    var value = DateTime.UnixEpoch.AddSeconds(fromPacket);
                    property.SetValue(Instance, value);
                    return true;
                }
            }
            //Strings
            if (property.PropertyType == typeof(string))
            {
                TSOVoltronString? attribute = getPropertyAttribute<TSOVoltronString>(property, out TSOVoltronValueTypes type);
                bool hasAttrib = attribute != null;
                if (!hasAttrib) // AUTOSELECT
                    type = TSOVoltronValueTypes.Pascal;

                //---STRING
                string destValue = ReadString(type, _bodyBuffer, attribute?.NullTerminatedMaxLength ?? 255);
                property.SetValue(Instance, destValue);
                return true;
            }
            //try deserialize object
            if (property.PropertyType.IsClass)
            {
                object? value = TSOVoltronSerializer.Deserialize(Stream, property.PropertyType);
                if (value != default)
                {
                    property.SetValue(Instance, value);
                    return true;
                }
            }
            return false;
        }

        private static TSOVoltronSerializerGraphItem? _lastGraphItem;
        /// <summary>
        /// Serializes the given <paramref name="property"/> to the <paramref name="Stream"/> using the value assigned on
        /// <paramref name="Instance"/>        
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="property"></param>
        /// <param name="Instance"></param>
        /// <returns>Value indicating whether writing is successful. Unsuccessful case would be a provided data type is not 
        /// supported</returns>
        public static bool WriteProperty(Stream Stream, PropertyInfo property, object? Instance)
        {
            _lastGraphItem = null;            

            bool hasAttrib = getPropertyAttribute<TSOVoltronValue>(property, out TSOVoltronValueTypes type) != default;
            object? myValue = property.GetValue(Instance);
            if (myValue == default)
            {
                _lastGraphItem = new(property.Name, property.PropertyType, null);
                return true;
            }
            var endianConverter = (EndianBitConverter)(
                    type == TSOVoltronValueTypes.BigEndian ?
                        EndianBitConverter.Big :
                        EndianBitConverter.Little);

            bool wroteValue = true;

            if (property.PropertyType == typeof(byte[]))
            {
                Stream.EmplaceBody((byte[])myValue);
                _lastGraphItem = new(property.Name,
                    typeof(byte[]), (byte[])myValue, $"{((byte[])myValue).Length} bytes.");
                return true;
            }

            Type PropertyType = property.PropertyType;
            while (PropertyType.IsEnum)
                PropertyType = Enum.GetUnderlyingType(PropertyType);

            //---NUMERICS
            if (PropertyType.IsAssignableTo(typeof(ushort)))
                Stream.EmplaceBody(endianConverter.GetBytes((ushort)myValue));
            else if (PropertyType == typeof(short))
                Stream.EmplaceBody(endianConverter.GetBytes((short)myValue));
            else if (PropertyType.IsAssignableTo(typeof(uint)))
                Stream.EmplaceBody(endianConverter.GetBytes((uint)myValue));
            else if (PropertyType == typeof(int))
                Stream.EmplaceBody(endianConverter.GetBytes((int)myValue));
            else if (PropertyType == typeof(byte))
                Stream.EmplaceBody((byte)myValue);
            else if (PropertyType == typeof(DateTime))
                Stream.EmplaceBody((uint)((DateTime)myValue).Minute); // probably not right
            else if (PropertyType == typeof(bool))
                Stream.EmplaceBody((byte)((bool)myValue ? 1 : 0));
            else wroteValue = false;

            if (wroteValue)
            {
                string valueString = myValue.ToString();
                if (property.PropertyType.IsEnum)                
                    valueString = Enum.GetName(property.PropertyType, myValue) ?? valueString;                
                _lastGraphItem = new(property.Name, PropertyType, myValue, valueString);
                return true;
            }
            if (!hasAttrib) // AUTOSELECT
                type = TSOVoltronValueTypes.Pascal;

            //---STRINGS            
            if (PropertyType == typeof(string[]))
            {
                foreach (var str in (string[])myValue)
                    WriteString(Stream, str, type);                
                return true;
            }
            else if (myValue is string myStringValue)
            {
                WriteString(Stream, myStringValue, type);
                _lastGraphItem = new(property.Name, typeof(string), myStringValue, myStringValue);
                return true;
            }

            //--SERIALIZABLES
            if (PropertyType.IsClass)
            {
                TSOVoltronSerializer.Serialize(Stream, myValue);
                _lastGraphItem = TSOVoltronSerializer.GetLastGraph() ?? new(property.Name, PropertyType, myValue);
                return true;
            }

            return wroteValue;
        }

        public static TSOVoltronSerializerGraphItem? GetLastGraph() => _lastGraphItem;
    }
}

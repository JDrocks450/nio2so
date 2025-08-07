using MiscUtil.Conversion;
using System.Reflection;
using System.Text;
using QuazarAPI.Networking.Data;
using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Serialization
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
        /// <exception cref="Exception">Pascal string not supported</exception>
        public static string ReadString(TSOVoltronValueTypes StringType, Stream Stream, int NullTerminatedMaxLength = 255, char NullTerminatorChar = '\0')
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
                    destValue = Stream.ReadBodyNullTerminatedString(NullTerminatedMaxLength, NullTerminatorChar);
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
        public static bool ReflectProperty(Stream Stream, PropertyInfo property, object Instance)
        {
            Stream _bodyBuffer = Stream;
            //check if this property is the body array property
            if (property.PropertyType == typeof(byte[]))
            {
                if (property.GetCustomAttribute<TSOVoltronBodyArray>() != default)
                { // it is.
                    if (property.PropertyType != typeof(byte[])) // not a byte[]
                        throw new CustomAttributeFormatException($"You applied the TSOVoltronBodyArray attribute to {property.PropertyType.Name}! Please ensure it matches byte[].");
                    int remainingData = (int)(Stream.Length - Stream.Position); // get remaining bytes count
                    byte[] destBuffer = new byte[remainingData];
                    _bodyBuffer.Read(destBuffer, 0, remainingData); // read into buffer
                    property.SetValue(Instance, destBuffer);
                    return true; // halt execution
                }
                else throw new NotImplementedException("byte[] is the destination type to deserialize, yet you didn't " +
                    "adorn the property with the TSOVoltronBodyArrayAtrribute! This is required to ensure you understand " +
                    "that it will take EVERY byte to the end of the packet body into that property and also for code-clarity.");
            }
            
            if (property.PropertyType.IsArray)
            {
                PropertyInfo? arrayLength = Instance.GetType().GetProperties().Where(x => x.GetCustomAttribute<TSOVoltronArrayLength>() != null)
                    .FirstOrDefault(y => y.GetCustomAttribute<TSOVoltronArrayLength>().ArrayPropertyName.ToLowerInvariant() == property.Name.ToLowerInvariant());
                if (arrayLength == null)
                    throw new InvalidDataException("ArrayLength property for this array was not found for the type: " + Instance.GetType().Name);
                uint count = Convert.ToUInt32(arrayLength.GetValue(Instance));
                Type arrayType = property.PropertyType.GetElementType();
                Array arrayVal = Array.CreateInstance(arrayType, count);
                for(int i = 0; i < count; i++)                
                    arrayVal.SetValue(TryNumericTypes(Stream, arrayType),i);
                property.SetValue(Instance, arrayVal);
                return true;
            }

            //Numbers
            if (TryNumericTypes(Stream, property, Instance)) return true;

            //Strings
            if (property.PropertyType == typeof(string))
            {
                TSOVoltronString? attribute = getPropertyAttribute<TSOVoltronString>(property, out TSOVoltronValueTypes type);
                bool hasAttrib = attribute != null;
                if (!hasAttrib) // AUTOSELECT
                    type = TSOVoltronValueTypes.Pascal;

                //---STRING
                string destValue = ReadString(type, _bodyBuffer, attribute?.NullTerminatedMaxLength ?? 255, attribute?.NullTerminatorChar ?? '\0');
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

        /// <summary>
        /// <inheritdoc cref="TryNumericTypes(Stream, PropertyInfo, object)"/>
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="PropertyType"></param>
        /// <param name="dataEndianMode"></param>
        /// <returns></returns>
        public static object TryNumericTypes(Stream Stream, Type PropertyType, Endianness dataEndianMode = Endianness.BigEndian)
        {
            bool readValue = true;

            //fully evaluate enum types to base value type
            while (PropertyType.IsEnum)
                PropertyType = Enum.GetUnderlyingType(PropertyType);

            //---NUMBERS
            if (PropertyType == typeof(byte))
            { // BYTE
                return (byte)Stream.ReadBodyByte();
            }
            else if (PropertyType == typeof(bool))
            { // TRUE FALSE
                byte value = (byte)Stream.ReadBodyByte();
                return value != 0;                
            }
            else if (PropertyType == typeof(ushort) || PropertyType == typeof(short))
            { // INT16/U16 rewrite 07/08/25
                if (PropertyType == typeof(ushort)) // uint16
                    return Stream.ReadBodyUshort(dataEndianMode); // read in an unsigned short
                else // int16
                { // no it wasn't. previous solution sucked. read 2 bytes from body and convert to int16 like a normal human being.
                    byte[] intBytes = new byte[2];
                    Stream.ReadAtLeast(intBytes, 2);
                    return ((EndianBitConverter)(dataEndianMode == Endianness.LittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big)).ToInt16(intBytes, 0);
                        
                }
                return true;
            }
            else if (PropertyType == typeof(uint) || PropertyType == typeof(int))
            {// INT32/U32 rewrite 07/08/25
                if (PropertyType == typeof(uint)) // uint32
                    return Stream.ReadBodyDword(dataEndianMode); // read in an unsigned int
                else // int32
                { // no it wasn't. previous solution sucked. read 4 bytes from body and convert to int32 like a normal human being.
                    byte[] intBytes = new byte[4];
                    Stream.ReadAtLeast(intBytes, 4);
                    return
                        ((EndianBitConverter)(dataEndianMode == Endianness.LittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big)).ToInt32(intBytes, 0);
                }
                return true;
            }
            else readValue = false;

            if (readValue) return true;

            if (PropertyType == typeof(DateTime))
            { // DATE TIME does this even work for tso pre alpha.. lol?
                uint fromPacket = Stream.ReadBodyDword();
                var value = DateTime.UnixEpoch.AddSeconds(fromPacket);
                return value;                
            }
            return false;
        }

        /// <summary>
        /// Tries to evaluate this property as a Numeric type and returns true if successful.
        /// <para/>Value is automatically applied to <paramref name="property"/> if available
        /// <para/>Supports:
        /// <code>byte, bool, ushort, short, uint, int, DateTime</code>
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="property"></param>
        /// <param name="Instance"></param>
        /// <returns></returns>
        static bool TryNumericTypes(Stream Stream, PropertyInfo property, object Instance)
        {
            //check member attribute for special considerations
            TSOVoltronValue? attribute = getPropertyAttribute<TSOVoltronValue>(property, out TSOVoltronValueTypes type);
            bool hasAttributes = attribute != default;
            //BigEndian by default!
            Endianness dataEndianMode = type == TSOVoltronValueTypes.LittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;
            bool readValue = true;

            Type PropertyType = property.PropertyType;
            //fully evaluate enum types to base value type
            while (PropertyType.IsEnum)
                PropertyType = Enum.GetUnderlyingType(PropertyType);            

            //---NUMBERS
            if (PropertyType == typeof(byte))
            { // BYTE
                property.SetValue(Instance, (byte)Stream.ReadBodyByte());
                return true;
            }
            else if (PropertyType == typeof(bool))
            { // TRUE FALSE
                byte value = (byte)Stream.ReadBodyByte();
                property.SetValue(Instance, value != 0);
                return true;
            }
            else if (PropertyType == typeof(ushort) || PropertyType == typeof(short))
            { // INT16/U16 rewrite 07/08/25
                if (PropertyType == typeof(ushort)) // uint16
                    property.SetValue(Instance, Stream.ReadBodyUshort(dataEndianMode)); // read in an unsigned short
                else // int16
                { // no it wasn't. previous solution sucked. read 2 bytes from body and convert to int16 like a normal human being.
                    byte[] intBytes = new byte[2];
                    Stream.ReadAtLeast(intBytes, 2);
                    property.SetValue(Instance,
                        ((EndianBitConverter)(dataEndianMode == Endianness.LittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big)).ToInt16(intBytes, 0));
                }
                return true;
            }
            else if (PropertyType == typeof(uint) || PropertyType == typeof(int))
            {// INT32/U32 rewrite 07/08/25
                if (PropertyType == typeof(uint)) // uint32
                    property.SetValue(Instance, Stream.ReadBodyDword(dataEndianMode)); // read in an unsigned int
                else // int32
                { // no it wasn't. previous solution sucked. read 4 bytes from body and convert to int32 like a normal human being.
                    byte[] intBytes = new byte[4];
                    Stream.ReadAtLeast(intBytes, 4);
                    property.SetValue(Instance,
                        ((EndianBitConverter)(dataEndianMode == Endianness.LittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big)).ToInt32(intBytes, 0));
                }
                return true;
            }
            else readValue = false;

            if (readValue) return true;

            if (PropertyType == typeof(DateTime))
            { // DATE TIME does this even work for tso pre alpha.. lol?
                uint fromPacket = Stream.ReadBodyDword();
                var value = DateTime.UnixEpoch.AddSeconds(fromPacket);
                property.SetValue(Instance, value);
                return true;
            }
            return false;
        }

        private static Stack<TSOVoltronSerializerGraphItem> _lastGraphItems = new();
        /// <summary>
        /// Serializes the given <paramref name="property"/> to the <paramref name="Stream"/> using the value assigned on the given
        /// <paramref name="Instance"/>        
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="property"></param>
        /// <param name="Instance"></param>
        /// <returns>Value indicating whether writing is successful. Unsuccessful case would be a provided data type is not 
        /// supported</returns>
        public static bool WriteProperty(Stream Stream, PropertyInfo property, object? Instance)
        {
            long startLen = Stream.Position;
            _lastGraphItems.Clear();            

            bool hasAttrib = getPropertyAttribute<TSOVoltronValue>(property, out TSOVoltronValueTypes type) != default;
            object? SerializeValue = property.GetValue(Instance);

            //**array length property handler
            {
                var arrLegAtt = property.GetCustomAttribute<TSOVoltronArrayLength>();
                if (arrLegAtt != null) { // it is one of these array length attributes
                    void error (string error) => throw new InvalidOperationException(error);
                    if (string.IsNullOrWhiteSpace(arrLegAtt.ArrayPropertyName))
                        error($"{arrLegAtt.ArrayPropertyName} is null");
                    //try to locate the desired property
                    PropertyInfo? foundArrayProperty = property?.DeclaringType?.GetProperty(arrLegAtt.ArrayPropertyName);
                    if (foundArrayProperty == null)
                        error($"{arrLegAtt.ArrayPropertyName} is doesn't exist on the type {property?.DeclaringType.Name ?? "not found!"}");
                    if (!foundArrayProperty.PropertyType.IsArray)
                        error($"{arrLegAtt.ArrayPropertyName} might very well be a property on the given type -- but it is not an array. Fix this.");
                    Array? myArrayValue = foundArrayProperty.GetValue(Instance) as Array;
                    if (myArrayValue == null)
                        error($"{arrLegAtt.ArrayPropertyName} is an array type, but its value is null. You really should not be doing this. It should be an empty array when using these attributes.");
                    SerializeValue = Convert.ChangeType(myArrayValue.Length, property.PropertyType); // set myvalue to be the length of the array
                }
            }

            if (SerializeValue == null) // handle NULLs!
            { // item is null -- handle this
                _lastGraphItems.Push(new(property, property.PropertyType, null, 0));
                return true;
            }

            //get endian converter by current culture
            var endianConverter = (EndianBitConverter)(
                    type == TSOVoltronValueTypes.BigEndian ?
                        EndianBitConverter.Big :
                        EndianBitConverter.Little);

            bool wroteValue = true;

            //optimization -- arrays are handled below, but that's slow and heavy-handed. just dump the array-call it a day. that rhymes ha ha ha ha ha ha ha ha
            if (property.PropertyType == typeof(byte[]))
            {
                Stream.EmplaceBody((byte[])SerializeValue);
                int length = ((byte[])SerializeValue).Length;
                _lastGraphItems.Push(new(property, typeof(byte[]), (byte[])SerializeValue, length, $"{length} bytes directly."));
                return true;
            }

            //--ARRAYS
            if (property.PropertyType.IsArray)
            {
                Array arr = (Array)property.GetValue(Instance);
                int i = 0;
                if (arr.Length <= 0)
                    _lastGraphItems.Push(new(property, arr.GetType(), arr, 0, "An empty array (0 bytes)"));
                foreach (var item in arr)
                {
                    if (!arr.GetType().GetElementType().IsClass) throw new NotImplementedException("Arrays with an element type that isn't a class isn't supported right now.");
                    startLen = Stream.Position;
                    TSOVoltronSerializer.Serialize(Stream, item);
                    _lastGraphItems.Push(TSOVoltronSerializer.GetLastGraph() ?? new(property.Name + $"[{i}]", item.GetType(), item, Stream.Position - startLen));
                    i++;
                }
                return true;
            }

            Type PropertyType = property.PropertyType;
            while (PropertyType.IsEnum)
                PropertyType = Enum.GetUnderlyingType(PropertyType);

            //---NUMERICS
            if (PropertyType.IsAssignableTo(typeof(ushort)))
                Stream.EmplaceBody(endianConverter.GetBytes((ushort)SerializeValue));
            else if (PropertyType == typeof(short))
                Stream.EmplaceBody(endianConverter.GetBytes((short)SerializeValue));
            else if (PropertyType.IsAssignableTo(typeof(uint)))
                Stream.EmplaceBody(endianConverter.GetBytes((uint)SerializeValue));
            else if (PropertyType == typeof(int))
                Stream.EmplaceBody(endianConverter.GetBytes((int)SerializeValue));
            else if (PropertyType == typeof(byte))
                Stream.EmplaceBody((byte)SerializeValue);
            else if (PropertyType == typeof(DateTime))
                Stream.EmplaceBody((uint)((DateTime)SerializeValue).Minute); // probably not right
            else if (PropertyType == typeof(bool))
                Stream.EmplaceBody((byte)((bool)SerializeValue ? 1 : 0));
            else wroteValue = false;

            if (wroteValue)
            {
                string valueString = SerializeValue.ToString();
                if (property.PropertyType.IsEnum)                
                    valueString = Enum.GetName(property.PropertyType, SerializeValue) ?? valueString;                
                _lastGraphItems.Push(new(property, PropertyType, SerializeValue, Stream.Position - startLen, valueString));
                return true;
            }
            if (!hasAttrib) // AUTOSELECT
                type = TSOVoltronValueTypes.Pascal;

            //---STRINGS            
            if (PropertyType == typeof(string[]))
            {
                foreach (var str in (string[])SerializeValue)
                    WriteString(Stream, str, type);                
                return true;
            }
            else if (SerializeValue is string myStringValue)
            {
                WriteString(Stream, myStringValue, type);
                _lastGraphItems.Push(new(property, typeof(string), myStringValue, Stream.Position - startLen, myStringValue));
                return true;
            }

            //--SERIALIZABLES
            if (PropertyType.IsClass)
            {
                TSOVoltronSerializer.Serialize(Stream, SerializeValue, property);
                _lastGraphItems.Push(TSOVoltronSerializer.GetLastGraph() ?? new(property, PropertyType, SerializeValue, Stream.Position - startLen));
                return true;
            }

            return wroteValue;
        }

        /// <summary>
        /// Gets all graph items off the stack then clears it
        /// </summary>
        /// <returns></returns>
        public static TSOVoltronSerializerGraphItem[] GetLastGraph()
        {
            if (TSOVoltronSerializer.CreatingSerializationGraphs)
            {
                var arr = _lastGraphItems.ToArray();
                _lastGraphItems.Clear();
                return arr;
            }
            return [];
        }
    }
}

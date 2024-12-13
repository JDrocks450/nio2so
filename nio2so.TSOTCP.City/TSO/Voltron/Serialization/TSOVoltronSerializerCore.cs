using MiscUtil.Conversion;
using System.Reflection;
using System.Text;
using QuazarAPI.Networking.Data;

namespace nio2so.TSOTCP.City.TSO.Voltron.Serialization
{
    internal static class TSOVoltronSerializerCore
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

        public static void WriteString(Stream Stream, string Text, TSOVoltronValueTypes type = TSOVoltronValueTypes.Pascal)
        {
            if (type == TSOVoltronValueTypes.Pascal)
            {
                Stream.EmplaceBody(0x80, 0x00);
                Stream.EmplaceBody(EndianBitConverter.Big.GetBytes((UInt16)Text.Length));
            }
            else Text += '\0';
            Stream.EmplaceBody(Encoding.UTF8.GetBytes(Text));
        }
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
                        byte[] strBytes = Stream.ReadBodyByteArray((int)len);
                        destValue = Encoding.UTF8.GetString(strBytes);
                    }
                    break;
                case TSOVoltronValueTypes.NullTerminated:
                    destValue = Stream.ReadBodyNullTerminatedString(NullTerminatedMaxLength);
                    break;
                case TSOVoltronValueTypes.Length_Prefixed_Byte:
                    {
                        int len = Stream.ReadBodyByte();
                        byte[] strBytes = Stream.ReadBodyByteArray((int)len);
                        destValue = Encoding.UTF8.GetString(strBytes);
                    }
                    break;
            }
            return destValue;
        }

        public static bool ReflectProperty(Stream Stream, PropertyInfo property, object? Instance)
        {
            Stream _bodyBuffer = Stream;
            //check if this property is the body array property
            if (property.GetCustomAttribute<TSOVoltronBodyArray>() != default)
            { // it is.
                if (property.PropertyType != typeof(Byte[]))
                    throw new CustomAttributeFormatException("You applied the TSOVoltronBodyArray attribute to ... not a byte[]! Are you testing me?");
                int remainingData = (int)(Stream.Length - Stream.Position);
                byte[] destBuffer = new byte[remainingData];
                _bodyBuffer.Read(destBuffer, 0, remainingData);
                property.SetValue(Instance, destBuffer);
                return true; // halt execution
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
                else if (PropertyType == typeof(UInt16) || PropertyType == typeof(Int16))
                {
                    ushort fromPacket = Stream.ReadBodyUshort(); // read an unsigned short
                    if (PropertyType == typeof(UInt16)) // is it even an unsigned short?
                        property.SetValue(Instance, fromPacket); // yeah
                    else property.SetValue(Instance, Convert.ToInt16(fromPacket)); // no it wasn't, convert it. uhh, i think this works?
                    return true;
                }
                else if (PropertyType == typeof(UInt32) || PropertyType == typeof(Int32))
                {
                    uint fromPacket = Stream.ReadBodyDword(); // read an unsigned int
                    if (PropertyType == typeof(UInt32)) // is it even an unsigned int?
                        property.SetValue(Instance, fromPacket); // yeah
                    else property.SetValue(Instance, Convert.ToInt32(fromPacket)); // no it wasn't, convert it. uhh, i think this works?
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
                    property.SetValue(Instance, value);
            }
            return false;
        }

        public static bool WriteProperty(Stream Stream, PropertyInfo property, Object? Instance)
        {
            bool hasAttrib = getPropertyAttribute<TSOVoltronValue>(property, out TSOVoltronValueTypes type) != default;
            object? myValue = property.GetValue(Instance);
            if (myValue == default) return true;
            var endianConverter = (EndianBitConverter)(
                    type == TSOVoltronValueTypes.BigEndian ?
                        EndianBitConverter.Big :
                        EndianBitConverter.Little);

            bool wroteValue = true;

            if (property.PropertyType == typeof(Byte[]))
            {
                Stream.EmplaceBody((byte[])myValue);
                return true;
            }

            Type PropertyType = property.PropertyType;
            while (PropertyType.IsEnum)
                PropertyType = Enum.GetUnderlyingType(PropertyType);

            //---NUMERICS
            if (PropertyType.IsAssignableTo(typeof(UInt16)))
                Stream.EmplaceBody(endianConverter.GetBytes((UInt16)myValue));
            else if (PropertyType == typeof(Int16))
                Stream.EmplaceBody(endianConverter.GetBytes((Int16)myValue));
            else if (PropertyType.IsAssignableTo(typeof(UInt32)))
                Stream.EmplaceBody(endianConverter.GetBytes((UInt32)myValue));
            else if (PropertyType == typeof(Int32))
                Stream.EmplaceBody(endianConverter.GetBytes((Int32)myValue));
            else if (PropertyType == typeof(byte))
                Stream.EmplaceBody((byte)myValue);
            else if (PropertyType == typeof(DateTime))
                Stream.EmplaceBody((uint)((DateTime)myValue).Minute); // probably not right
            else if (PropertyType == typeof(Boolean))
                Stream.EmplaceBody((byte)((bool)myValue ? 1 : 0));
            else wroteValue = false;

            if (wroteValue) return true;
            if (!hasAttrib) // AUTOSELECT
                type = TSOVoltronValueTypes.Pascal;

            //---STRINGS            
            if (PropertyType == typeof(string[]))
            {
                foreach (var str in (string[])myValue)
                    WriteString(Stream, str, type);
                return true;
            }
            else if (myValue is String myStringValue)
            {
                WriteString(Stream, myStringValue, type);
                return true;
            }

            //--SERIALIZABLES
            if (PropertyType.IsClass)            
                TSOVoltronSerializer.Serialize(Stream, myValue);            

            return wroteValue;
        }
    }
}

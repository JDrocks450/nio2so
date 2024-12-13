using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using QuazarAPI.Networking.Data;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Serialization
{
    internal static class TSOSerializerCore
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
            }
            else if (myValue is String myStringValue)
                WriteString(Stream, myStringValue, type);

            return wroteValue;
        }
    }

    internal static class TSOSerializer
    {                
        public static void Serialize<T>(Stream Stream, T Object)
        {
            foreach (var property in Object.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null) continue;
                //**serializable property
                TSOSerializerCore.WriteProperty(Stream, property, Object);
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

        public static T Deserialize<T>(Stream Stream)
        {

        }
    }
}

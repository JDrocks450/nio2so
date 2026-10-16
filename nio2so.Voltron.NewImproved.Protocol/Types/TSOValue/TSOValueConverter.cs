using nio2so.Formats.TSOData;
using nio2so.Voltron.NewImproved.Protocol.Regulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class TSOValueAttribute : Attribute
    {        
        public TSOValueAttribute(TSO_NewImproved_TSOValueBase_CLSIDs TypeCLSID, Type NativeType)
        {
            this.TypeCLSID = TypeCLSID;
            this.RuntimeType = NativeType;
        }
        public TSO_NewImproved_TSOValueBase_CLSIDs TypeCLSID { get; }
        public Type RuntimeType { get; }
    }

    internal class TSOValueConverter
    {
        static ITSOValue? convertBase(object value)
        {
            if (value is String)
                return new TSOValue_String((string)value);

            if (value is bool)
                return new TSOValue_Boolean((bool)value);

            if (value is byte)
                return new TSOValue_Uint8((byte)value);

            if (value is sbyte)
                return new TSOValue_Sint8((sbyte)value);

            if (value is short)
                return new TSOValue_Sint16((short)value);

            if (value is ushort)
                return new TSOValue_Uint16((ushort)value);

            if (value is int)
                return new TSOValue_Sint32((int)value);

            if (value is uint)
                return new TSOValue_Uint32((uint)value);

            if (value is long)
                return new TSOValue_Sint64((long)value);

            if (value is ulong)
                return new TSOValue_Uint64((ulong)value);

            return null;
        }

        public static ITSOValue Convert(object value)
        {
            ITSOValue? val = convertBase(value);
            if (val == null)
                throw new NotSupportedException($"Type {value.GetType()} not supported.");
            return val;
        }

        public static bool TryConvert(object value, out ITSOValue? Converted)
        {
            Converted = convertBase(value);
            return (Converted != null);
        }
    }
}

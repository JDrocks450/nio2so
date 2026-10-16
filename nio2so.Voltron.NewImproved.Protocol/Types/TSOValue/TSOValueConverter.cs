using nio2so.Formats.TSOData;
using nio2so.Voltron.NewImproved.Protocol.Regulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
        static ITSOValue? convertBase(string? typeHint, object value)
        {
            //todo: map these types...? is it even worth the effort? this works fine for now tho kinda ugly

            //** convert to type based on typeHint
            if (!string.IsNullOrWhiteSpace(typeHint))
            {
                ITSOValue? returnType = typeHint.ToLowerInvariant() switch
                {                    
                    "string" => new TSOValue_String(System.Convert.ToString(value)!),

                    "bool" => new TSOValue_Boolean(System.Convert.ToBoolean(value)),

                    "uint8" => new TSOValue_Uint8(System.Convert.ToByte(value)),
                    "sint8" => new TSOValue_Sint8(System.Convert.ToSByte(value)),

                    "uint16" => new TSOValue_Uint16(System.Convert.ToUInt16(value)),
                    "sint16" => new TSOValue_Sint16(System.Convert.ToInt16(value)),

                    "uint32" => new TSOValue_Uint32(System.Convert.ToUInt32(value)),
                    "sint32" => new TSOValue_Sint32(System.Convert.ToInt32(value)),

                    "uint64" => new TSOValue_Uint64(System.Convert.ToUInt64(value)),
                    "sint64" => new TSOValue_Sint64(System.Convert.ToInt64(value)),

                    _ => null
                };
                if (returnType != null)
                    return returnType;
                //yikes. try hintless logic .. if this falls through we're missing types or have type confusion
            }

            // hintless logic, just try to convert it based on runtime type
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
            ITSOValue? val = convertBase(null, value);
            if (val == null)
                throw new NotSupportedException($"Type {value.GetType()} not supported.");
            return val;
        }

        /// <summary>
        /// <inheritdoc cref="TryConvert(object, out ITSOValue?)"/>
        /// <para/>This has added safety by ensuring the <paramref name="value"/> is wrapped in the <paramref name="ExpectedType"/> <see cref="ITSOValue"/>. Basic type-cast only - always be careful to convert your variable type YOURSELF this is a safety net.
        /// </summary>
        /// <param name="ExpectedType"></param>
        /// <param name="value"></param>
        /// <param name="Converted"></param>
        /// <returns></returns>
        public static bool TryConvert(string TypeHint, object value, out ITSOValue? Converted)
        {
            Converted = convertBase(TypeHint, value);
            return (Converted != null);
        }

        /// <summary>
        /// Converts the given <see cref="Type"/> to a <see cref="ITSOValue"/> type. Basic types only. You cannot use this with structs.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Converted"></param>
        /// <returns></returns>
        public static bool TryConvert(object value, out ITSOValue? Converted)
        {
            Converted = convertBase(null, value);
            return (Converted != null);
        }
    }
}

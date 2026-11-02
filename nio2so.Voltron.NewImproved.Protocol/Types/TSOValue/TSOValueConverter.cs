using nio2so.Formats.TSOData;
using System.Collections;
using System.Text.Json;

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

    internal abstract class TSOValueConverterBase<TInput, TOutput> : ITSODataValueConverter<TInput, TOutput>
    {

        public static Type TypeFromCLSID(TSO_NewImproved_TSOValueBase_CLSIDs ValueCLSID)
        {
            switch (ValueCLSID)
            {
                //***VALUE TYPES

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint8:
                    return typeof(TSOValue_Uint8);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint8:
                    return typeof(TSOValue_Sint8);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint16:
                    return typeof(TSOValue_Uint16);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint16:
                    return typeof(TSOValue_Sint16);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint32:
                    return typeof(TSOValue_Uint32);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint32:
                    return typeof(TSOValue_Sint32);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Uint64:
                    return typeof(TSOValue_Uint64);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Sint64:
                    return typeof(TSOValue_Sint64);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_String:
                    return typeof(TSOValue_String);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Bool:
                    return typeof(TSOValue_Boolean);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Property:
                    return typeof(TSOValue_TSODataStructProperty);

                    //***VECTORS

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint8:
                    return typeof(TSOValue_Uint8Vector);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint8:
                    return typeof(TSOValue_Sint8Vector);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint16:
                    return typeof(TSOValue_Uint16Vector);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint16:
                    return typeof(TSOValue_Sint16Vector);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint32:
                    return typeof(TSOValue_Uint32Vector);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint32:
                    return typeof(TSOValue_Sint32Vector);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Uint64:
                    return typeof(TSOValue_Uint64Vector);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Sint64:
                    return typeof(TSOValue_Sint64Vector);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_String:
                    return typeof(TSOValue_StringVector);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueVector_Property:
                    return typeof(TSOValue_PropertyVector);

                //***MAPS -- INCOMPLETE

#if false
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Uint8:
                    return typeof(TSOValue_Uint8);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Sint8:
                    return typeof(TSOValue_Sint8);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Uint16:
                    return typeof(TSOValue_Uint16);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Sint16:
                    return typeof(TSOValue_Sint16);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Uint32:
                    return typeof(TSOValue_Uint32Map);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Sint32:
                    return typeof(TSOValue_Sint32);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Uint64:
                    return typeof(TSOValue_Uint64);
                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Sint64:
                    return typeof(TSOValue_Sint64);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_String:
                    return typeof(TSOValue_String);

                case TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Bool:
                    return typeof(TSOValue_BooleanMap);
#endif
                default:
                    throw new NotImplementedException(ValueCLSID.ToString());
            }
        }

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

                    "iunknown" => new TSOValue_iunknown((byte[])value),
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
        static ITSOValueVector convertBaseArray(string typeHint, ITSOValue[] Source)
        {
            //todo: map these types...? is it even worth the effort? this works fine for now tho kinda ugly

            //** convert to type based on typeHint
            if (string.IsNullOrWhiteSpace(typeHint))
                throw new ArgumentException(nameof(typeHint));
            ITSOValueVector? returnType = typeHint.ToLowerInvariant() switch
            {
                "string" => new TSOValue_StringVector(Source.Select(x => System.Convert.ToString(x.GetValue())!).ToArray()),

                "bool" => new TSOValue_BooleanVector(Source.Select(x => System.Convert.ToBoolean(x.GetValue())).ToArray()),

                "uint8" => new TSOValue_Uint8Vector(Source.Select(x => System.Convert.ToByte(x.GetValue())).ToArray()),
                "sint8" => new TSOValue_Sint8Vector(Source.Select(x => System.Convert.ToSByte(x.GetValue())).ToArray()),

                "uint16" => new TSOValue_Uint16Vector(Source.Select(x => System.Convert.ToUInt16(x.GetValue())).ToArray()),
                "sint16" => new TSOValue_Sint16Vector(Source.Select(x => System.Convert.ToInt16(x.GetValue())).ToArray()),

                "uint32" => new TSOValue_Uint32Vector(Source.Select(x => System.Convert.ToUInt32(x.GetValue())).ToArray()),
                "sint32" => new TSOValue_Sint32Vector(Source.Select(x => System.Convert.ToInt32(x.GetValue())).ToArray()),

                "uint64" => new TSOValue_Uint64Vector(Source.Select(x => System.Convert.ToUInt64(x.GetValue())).ToArray()),
                "sint64" => new TSOValue_Sint64Vector(Source.Select(x => System.Convert.ToInt64(x.GetValue())).ToArray()),

                _ => null
            };
            if (returnType == null)
                throw new InvalidDataException($"Type: {typeHint} could not be made into a {nameof(ITSOValueVector)}");
            return returnType;
        }

        static ITSOValueMap convertBaseMap(string typeHint, params KeyValuePair<uint,ITSOValue>[] Pairs)
        {
            //todo: map these types...? is it even worth the effort? this works fine for now tho kinda ugly

            //** convert to type based on typeHint
            if (string.IsNullOrWhiteSpace(typeHint))
                throw new ArgumentException(nameof(typeHint));
            ITSOValueMap? returnType = typeHint.ToLowerInvariant() switch
            {
                /*"string" => new TSOValue_StringVector(Source.Select(x => System.Convert.ToString(x.GetValue())!).ToArray()),

                "uint8" => new TSOValue_Uint8Vector(Source.Select(x => System.Convert.ToByte(x.GetValue())).ToArray()),
                "sint8" => new TSOValue_Sint8Vector(Source.Select(x => System.Convert.ToSByte(x.GetValue())).ToArray()),

                "uint16" => new TSOValue_Uint16Vector(Source.Select(x => System.Convert.ToUInt16(x.GetValue())).ToArray()),
                "sint16" => new TSOValue_Sint16Vector(Source.Select(x => System.Convert.ToInt16(x.GetValue())).ToArray()),

                
                "sint32" => new TSOValue_Sint32Vector(Source.Select(x => System.Convert.ToInt32(x.GetValue())).ToArray()),

                "uint64" => new TSOValue_Uint64Vector(Source.Select(x => System.Convert.ToUInt64(x.GetValue())).ToArray()),
                "sint64" => new TSOValue_Sint64Vector(Source.Select(x => System.Convert.ToInt64(x.GetValue())).ToArray()),*/
                "bool" => new TSOValue_BooleanMap(Pairs.Select(x => new TSOValue_MapKeyValuePair<bool>(x.Key, System.Convert.ToBoolean(x.Value.GetValue()))).ToArray()),
                "uint32" => new TSOValue_Uint32Map(Pairs.Select(x => new TSOValue_MapKeyValuePair<uint>(x.Key, System.Convert.ToUInt32(x.Value.GetValue()))).ToArray()),
                _ => null
            };
            if (returnType == null)
                throw new InvalidDataException($"Type: {typeHint} could not be made into a {nameof(ITSOValueMap)}");
            return returnType;
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

        public static bool TryConvertArray(string TypeHint, object Value, out ITSOValueVector TypedList)
        {
            if (Value is not ICollection)
                throw new InvalidCastException();
            ICollection list = (ICollection)Value;
            ITSOValue[] valueArray = new ITSOValue[list.Count];
            int i = 0;
            foreach(var value in list)
            {
                TryConvert(TypeHint, value, out ITSOValue? converted);
                valueArray[i] = converted;
                i++;
            }
            TypedList = convertBaseArray(TypeHint, valueArray);
            return true;
        }

        public static bool TryConvertMap(string TypeHint, object Value, out ITSOValueMap Map)
        {
            ICollection< KeyValuePair<uint, object>> sourceValue = null ;
            if (Value is IDictionary<string, object> expando) // expandoObject -- convert to int - type map            
                sourceValue = expando.Select(x => new KeyValuePair<uint, object>(System.Convert.ToUInt32(x.Key), x.Value)).ToArray();
            else if (Value is not ICollection<KeyValuePair<uint, object>>)
                throw new InvalidCastException();
            else sourceValue = (ICollection< KeyValuePair<uint, object>>)Value;

            KeyValuePair<uint,ITSOValue>[] valueArray = new KeyValuePair<uint, ITSOValue>[sourceValue.Count];
            int i = 0;
            foreach (var pair in sourceValue)
            {
                TryConvert(TypeHint, pair.Value, out ITSOValue? converted);
                valueArray[i] = new KeyValuePair<uint,ITSOValue>(pair.Key, converted);
                i++;
            }
            Map = convertBaseMap(TypeHint, valueArray);
            return true;
        }        

        public abstract TOutput Convert2(TInput Input);
        public abstract bool TryConvert2(string TypeHint, TInput Input, out TOutput? Output);
    }

    internal class TSOValueConverter : TSOValueConverterBase<object, ITSOValue>
    {       
        public override ITSOValue Convert2(object Input) => Convert(Input);

        public override bool TryConvert2(string TypeHint, object Input, out ITSOValue? Output) => TryConvert(TypeHint, Input, out Output);
    }
    internal class TSOValueJsonConverter : TSOValueConverterBase<JsonElement, ITSOValue>
    {
        public override ITSOValue Convert2(JsonElement Input) => Convert(Input);

        public override bool TryConvert2(string TypeHint, JsonElement Input, out ITSOValue? Output) => TryConvert(TypeHint, Input, out Output);
    }
}

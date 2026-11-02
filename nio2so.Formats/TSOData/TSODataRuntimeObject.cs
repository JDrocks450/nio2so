using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using System.Xml.Linq;

namespace nio2so.Formats.TSOData
{
    public interface ITSODataValueConverter<TInput, TOutput>
    {
        TOutput? Convert2(TInput Input);
        bool TryConvert2(string TypeHint, TInput Input, out TOutput? Output);
    }

    public class TSODataBasicJsonConverter : ITSODataValueConverter<JsonElement, object>
    {
        public ITSODataStruct ThisType { get; }

        public TSODataBasicJsonConverter(ITSODataStruct ThisType)
        {
            this.ThisType = ThisType;
        }

        object? ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    IDictionary<string, object?> expando = new ExpandoObject();
                    foreach (var prop in element.EnumerateObject())
                    {
                        expando[prop.Name] = ConvertJsonElement(prop.Value);
                        /*
                        TSODataField? fieldInfo = ThisType.GetFieldByName(prop.Name);
                        if (fieldInfo == null)
                            throw new NullReferenceException("FieldInfo is null.");
                        if (!TryConvert2(fieldInfo.Classification.ToString(), prop.Value, out object? value) || value == null)
                            throw new Exception("Value not converted.");
                        expando[prop.Name] = value;*/
                    }
                    return (ExpandoObject)expando;

                case JsonValueKind.Array:
                    return element.EnumerateArray()
                        .Select(ConvertJsonElement)
                        .ToList();

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long l))
                        return l;

                    return element.GetDouble();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;

                default:
                    return element.ToString();
            }
        }

        public object? Convert2(JsonElement Input) => ConvertJsonElement(Input);

        public bool TryConvert2(string TypeHint, JsonElement Input, out object? Output)
        {
            Output = ConvertJsonElement(Input);
            return Output != null;
        }
    }

    /// <summary>
    /// Provides access to instances of a <see cref="ITSODataStruct"/> at runtime, keeping the <see cref="TSODataStruct"/> and dynamic object instance together
    /// </summary>
    public class TSODataRuntimeObject
    {
        /// <summary>
        /// Creates a new instance of <see cref="TSODataRuntimeObject"/> with the given <paramref name="Structure"/> and dy <paramref name="Instance"/> of that structure
        /// </summary>
        /// <param name="Structure"></param>
        /// <param name="Instance"></param>
        /// <exception cref="ArgumentException"></exception>
        public TSODataRuntimeObject(ITSODataStruct Structure, object Instance, ITSODataValueConverter<JsonElement, object>? ValueConverter = default)
        {
            this.Structure = Structure;  
            valueConverter = new TSODataBasicJsonConverter(Structure);          
            this.Instance = Instance;            
        }
        /// <summary>
        /// Accesses the <see cref="Structure"/> <see cref="ITSODataStruct.NameID"/> property
        /// </summary>
        public uint NameID => Structure.NameID;
        /// <summary>
        /// The TSODataDefinition Structure this object is an instance of
        /// </summary>
        public ITSODataStruct Structure { get; }
        /// <summary>
        /// The dynamic object instance of the <see cref="Structure"/>
        /// </summary>
        public object Instance
        {
            get => _instance;
            private set
            {
                if (value == null)
                    throw new ArgumentException(nameof(Instance));
                if (value is JsonElement)
                    value = (ExpandoObject)valueConverter.Convert2((JsonElement)value);
                if (value is not ExpandoObject)
                    throw new ArgumentException("Currently only compatible with ExpandoObject", nameof(Instance));
                _instance = (ExpandoObject)value;

                //sanitize
                string[] props = GetObjectAsDictionary().Where(x => x.Value is JsonElement).Select(y => y.Key).ToArray();
                foreach (var prop in props)                
                    GetObjectAsDictionary()[prop] = valueConverter.Convert2((JsonElement)GetObjectAsDictionary()[prop]);                  
            }
        }
        private ExpandoObject _instance;
        private readonly ITSODataValueConverter<JsonElement, object>? valueConverter;

        public IDictionary<string, object> GetObjectAsDictionary() => GetObjectAsExpando();
        public ExpandoObject GetObjectAsExpando() => _instance;
        public dynamic GetObjectAsDynamic() => Instance;
        public T GetObjectAs<T>() => (T)Instance;

        /// <summary>
        /// Runs an algorithm that ensures all properties on the <see cref="Instance"/> are fully deserialized.
        /// </summary>
        private static void EnsureDeserialized(object Instance)
        {
            
            if (Instance is not ExpandoObject)
                throw new ArgumentException("Currently only compatible with ExpandoObject", nameof(Instance));
        }        

        /// <summary>
        /// Accesses a property on the <see cref="Instance"/> by the <see cref="TSODataField.TypeID"/> of the property.
        /// <para/>Could be slightly slower, maybe use String overload in batch processes
        /// </summary>
        /// <param name="FieldID"></param>
        /// <param name="StructField"></param>
        /// <returns></returns>
        public object? GetFieldValue(uint FieldID, out TSODataField StructField)
        {
            StructField = Structure.GetFieldByID(FieldID);
            return GetFieldValue(StructField.NameString);
        }
        /// <summary>
        /// Accesses a property on the <see cref="Instance"/> by the <see cref="TSODataField.TypeString"/> of the property
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public object? GetFieldValue(string FieldName) => GetObjectAsDictionary()[FieldName];
        /// <summary>
        /// <inheritdoc cref="GetFieldValue(string)"/>
        /// </summary>
        /// <param name="FieldInfo"></param>
        /// <returns></returns>
        public object? GetFieldValue(TSODataField FieldInfo) => GetFieldValue(FieldInfo.NameString);
        /// <summary>
        /// Sets the value of a <see cref="TSODataField"/> from the <see cref="Structure"/> onto the <see cref="Instance"/>
        /// <para/>Danger: match the expected type of the <see cref="TSODataField.TypeString"/> property or this can cause problems!
        /// </summary>
        /// <param name="FieldID"></param>
        /// <param name="Value"></param>
        public void SetFieldValue(uint FieldID, object Value) => SetFieldValue(Structure.GetFieldByID(FieldID).NameString, Value);
        /// <summary>
        /// <inheritdoc cref="SetFieldValue(uint, object)"/>
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="Value"></param>
        public void SetFieldValue(string FieldName, object Value) => GetObjectAsDictionary()[FieldName] = Value;
        /// <summary>
        /// <inheritdoc cref="SetFieldValue(string, object)"/>
        /// </summary>
        /// <param name="FieldInfo"></param>
        /// <param name="Value"></param>
        public void SetFieldValue(TSODataField FieldInfo, object Value)
        {
            if (FieldInfo.Classification != TSODataFieldClassification.SingleField && Value is not ICollection)
                throw new InvalidCastException("Cannot set Map or TypedList to a value that doesn't implement ICollection");
            SetFieldValue(FieldInfo.NameString, Value);
        }

        public override string ToString() => Structure.ToString() ?? base.ToString();

        /// <summary>
        /// Copies to the <paramref name="Original"/> object's <see cref="TSODataRuntimeObject.Instance"/> properties to a new instance.
        /// <para/>This function can be used to change the type of the Destination struct instance. If using <see cref="TSODerivedStruct"/>, the Parent type of 
        /// both types must match.
        /// <para/>Returned type is guaranteed to be <paramref name="NewType"/>
        /// </summary>
        /// <param name="NewType"></param>
        /// <param name="Original"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public static TSODataRuntimeObject CopyTo(ITSODataStruct NewType, TSODataRuntimeObject Original)
        {
            ITSODataStruct type1 = Original.Structure;
            uint type2 = NewType.NameID;

            // find the original file types for these two objects

            if (type1.IsDerived)
                if (!type1.GetSourceFile().TryGetStructByID((type1 as TSODerivedStruct).ParentID, out type1))
                    throw new InvalidOperationException("Failed to dereference this Derived Struct's original type: " + type1);

            if (NewType.IsDerived)
                type2 = (NewType as TSODerivedStruct).ParentID;

            if (type1.NameID != type2) // same source type
                throw new InvalidOperationException($"{type1.NameString} is not compatible with type ID: {type2}.");

            //safe to proceed

            if (!Original.Structure.GetSourceFile().TryGetObject(NewType.NameID, out var Copied))
                throw new InvalidOperationException("Failed to create an object of type: "+ NewType);            

            foreach(var field in NewType.Fields)
            {
                object? value = Original.GetFieldValue(field.NameID, out TSODataField? FieldInfo);

                if (value == null || FieldInfo == null)
                    throw new NullReferenceException($"Failed to get Field value/information for field: " + field.NameString);

                Copied.SetFieldValue(field.NameID, value);
            }

            //guard function
            if (Copied.Structure.NameID != NewType.NameID)
                throw new InvalidDataException($"Instance created: {Copied.Structure} does not match the requested type: {NewType}");

            return Copied;
        }        
    }
}

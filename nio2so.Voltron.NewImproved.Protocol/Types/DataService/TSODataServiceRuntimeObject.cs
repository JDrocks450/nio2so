using nio2so.Formats.TSOData;
using System.Dynamic;

namespace nio2so.Voltron.NewImproved.Protocol.Types.DataService
{
    /// <summary>
    /// Provides access to instances of a <see cref="ITSODataStruct"/> at runtime, keeping the TSODataStruct and dynamic object instance together
    /// </summary>
    internal class TSODataServiceRuntimeObject
    {
        /// <summary>
        /// Creates a new instance of <see cref="TSODataServiceRuntimeObject"/> with the given <paramref name="Structure"/> and dy <paramref name="Instance"/> of that structure
        /// </summary>
        /// <param name="Structure"></param>
        /// <param name="Instance"></param>
        /// <exception cref="ArgumentException"></exception>
        public TSODataServiceRuntimeObject(ITSODataStruct Structure, object Instance)
        {
            this.Structure = Structure;
            if (Instance == null)
                throw new ArgumentException(nameof(Instance));
            if (Instance is not ExpandoObject)
                throw new ArgumentException("Currently only compatible with ExpandoObject", nameof(Instance));
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
        public object Instance { get; }

        public ExpandoObject? GetObjectAsExpando() => Instance as ExpandoObject;
        public dynamic GetObjectAsDynamic() => Instance;
        public T GetObjectAs<T>() => (T)Instance;

        /// <summary>
        /// Accesses a property on the <see cref="Instance"/> by the <see cref="TSODataField.TypeID"/> of the property
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
        public object? GetFieldValue(string FieldName) => GetObjectAsExpando()?.ToDictionary()[FieldName];

        public override string ToString() => Structure.ToString() ?? base.ToString();
    }
}

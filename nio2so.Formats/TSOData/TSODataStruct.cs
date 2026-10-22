using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public interface ITSODataNamedObject
    {
        /// <summary>
        /// The ID of the string giving this type its name
        /// </summary>
        [JsonIgnore] uint NameID { get; }
        /// <summary>
        /// The de-referenced <see cref="NameID"/> property, the string value
        /// </summary>
        string NameString { get; }
    }

    public interface ITSODataStruct : ITSODataStringIDAccessor
    {
        bool IsDerived => this is TSODerivedStruct;
        /// <summary>
        /// Gets the fields on the <see cref="ITSODataStruct"/>
        /// <para/>For <see cref="TSODerivedStruct"/> objects, this is a list of <see cref="TSOFieldMaskValues.Keep"/> properties.
        /// </summary>
        List<TSODataField> Fields { get; }
        /// <summary>
        /// The ID of the string giving this type its name
        /// </summary>
        uint NameID { get; }
        /// <summary>
        /// If this is <see cref="IsDerived"/> Struct, gets the base type <see cref="TSODataStruct"/> ID, otherwise returns <see cref="TSODataObject.NameID"/>
        /// </summary>
        /// <returns></returns>
        uint GetBaseStructID();
        /// <summary>
        /// The de-referenced <see cref="NameID"/> property, the string value
        /// </summary>
        string NameString => GetStringByID(NameID).Value;
        TSODataFile GetSourceFile();

        TSODataField? GetFieldByName(string propertyName) => Fields.FirstOrDefault(x => x.NameString == propertyName);
        TSODataField? GetFieldByID(uint FieldID) => Fields.FirstOrDefault(x => x.NameID == FieldID);        
    }

    public class TSODataStruct : TSODataObject, ITSODataStruct
    {
        public TSODataStruct(uint strID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = strID;
        }
        public uint FieldCount => (uint)Fields.Count;
        public List<TSODataField> Fields { get; } = new();
        public uint GetBaseStructID() => NameID;
    }
}

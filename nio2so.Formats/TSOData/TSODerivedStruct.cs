using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public class TSODerivedStruct : TSODataObject
    {
        public TSODerivedStruct(uint myNameID, uint parentNameID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = myNameID;
            ParentID = parentNameID;
        }

        public string ParentString => ParentFile.Strings[ParentID].Value;
        [JsonIgnore]
        public uint ParentID { get; set; }
        [JsonIgnore]
        public uint FieldMasksCount => (uint)FieldMasks.Count;
        public List<TSOFieldMask> FieldMasks { get; } = new();
    }
}

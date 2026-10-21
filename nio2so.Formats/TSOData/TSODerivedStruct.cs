using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public class TSODerivedStruct : TSODataObject, ITSODataStruct
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

        public List<TSODataField> Fields
        {
            get
            {
                if (!ParentFile.TryGetStructByID(ParentID, out ITSODataStruct Struct))
                    throw new InvalidOperationException($"ParentID: {ParentID} was not found.");
                List<TSODataField> returnList = new();
                foreach (var Field in Struct.Fields)
                {
                    var mask = FieldMasks.FirstOrDefault(x => x.NameID == Field.NameID);
                    if (mask != null)// && mask.Values == TSOFieldMaskValues.Keep)
                    {
                        returnList.Add(Field);
                        continue;
                    }
                }
                return returnList;
            }
        }
    }
}
using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public class TSODataField : TSODataObject
    {
        public TSODataField(uint fieldID, TSODataFieldClassification classific, uint typeStrID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = fieldID;
            Classification = classific;
            TypeID = typeStrID;
        }

        public string TypeString => ParentFile.Strings[TypeID].Value;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TSODataFieldClassification Classification { get; set; }
        [JsonIgnore]
        public uint TypeID { get;set; }             
    }
}

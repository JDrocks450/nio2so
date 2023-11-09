using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public class TSOFieldMask : TSODataObject
    {
        public TSOFieldMask(uint fieldMaskID, TSOFieldMaskValues value)
        {
            ParentFile = TSODataImporter.Current;
            NameID = fieldMaskID;
            Values = value;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TSOFieldMaskValues Values { get; set; }
    }
}

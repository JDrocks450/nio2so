using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public class TSODataString
    {
        public TSODataString(uint ID, string value, TSODataStringCategories category)
        {
            this.ID = ID;
            Value = value;
            Category = category;
        }

        public uint ID { get; set; }
        public string Value { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TSODataStringCategories Category { get; set; }

        public override string ToString() => Value;
    }
}

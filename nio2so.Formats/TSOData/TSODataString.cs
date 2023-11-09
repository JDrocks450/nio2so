using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public class TSODataString
    {
        public TSODataString(string value, TSODataStringCategories category)
        {
            Value = value;
            Category = category;
        }

        public string Value { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TSODataStringCategories Category { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public abstract class TSODataObject
    {
        protected TSODataFile ParentFile { get; set; }
        [JsonIgnore]
        public uint NameID { get; set; }
        public string NameString => ParentFile.Strings[NameID].Value;
    }
}

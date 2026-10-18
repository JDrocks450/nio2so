using Microsoft.VisualBasic;
using System.Text.Json.Serialization;

namespace nio2so.Formats.TSOData
{
    public abstract class TSODataObject : ITSODataStringIDAccessor, ITSODataNamedObject
    {
        protected TSODataFile ParentFile { get; set; }
        [JsonIgnore]
        public uint NameID { get; set; }
        public string NameString => GetStringByID(NameID).Value;

        public TSODataString GetStringByID(uint ID) => ParentFile.GetStringByID(ID);
        public uint GetIDByString(string Value) => ParentFile.GetIDByString(Value);

        public override string ToString()
        {
            return $"{GetType().Name} {NameString} ({NameID:X})";
        }

        public TSODataFile GetSourceFile() => ParentFile;
    }
}

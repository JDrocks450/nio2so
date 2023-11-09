using System.Text.Json;

namespace nio2so.Formats.TSOData
{
    /// <summary>
    /// Check out the file structure here: <see href="http://wiki.niotso.org/TSOData"/>
    /// </summary>
    public class TSODataFile : ITSOImportable
    {
        public DateTime TimeStamp { get; set; }
        public uint LevelOneStructsCount => (uint)LevelOneStructs.Count;
        public List<TSODataStruct> LevelOneStructs { get; } = new();
        public uint LevelTwoStructsCount => (uint)LevelTwoStructs.Count;
        public List<TSODataStruct> LevelTwoStructs { get; } = new();
        public uint DerivedStructsCount => (uint)DerivedStructs.Count;
        public List<TSODerivedStruct> DerivedStructs { get; } = new();
        public uint StringsCount => (uint)Strings.Count;
        public Dictionary<uint,TSODataString> Strings { get; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
            /* return
                $"This TSODataFile was built on {TimeStamp}\n" +
                $"---\n" +
                $"1st Level Structures ({LevelOneStructsCount}) {{\n" +
                $"  {string.Join('\n', LevelOneStructs)}\n" +
                $"}}\n" +
                $"2nd Level Structures ({LevelTwoStructsCount}) {{\n" +
                $"  {string.Join('\n', LevelTwoStructs)}\n" +
                $"}}\n" +
                $"Derived Structures ({DerivedStructsCount}) {{\n" +
                $"  {string.Join('\n', DerivedStructs)}\n" +
                $"}}\n" +
                $"Strings (for Reference) ({StringsCount}) {{\n" +
                $"  {string.Join('\n', Strings.Select(x => $"[{x.Key}] {x.Value.Value} ({x.Value.Category})"))}" +
                $"}}\n" +
                $"End of file."; */            
        }
    }
}

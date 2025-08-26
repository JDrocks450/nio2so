namespace nio2so.Formats.CST
{
    public record CSTValue(string StringValue)
    {
        public string Comment { get; set; } = "";

        public override string ToString()
        {
            return StringValue;
        }
    }
    public class CSTFile : Dictionary<string, CSTValue>, ITSOImportable
    {
        public string FilePath { get; set; }
        public bool Populated { get; internal set; } = false;
        public void Populate() => CSTImporter.PopulateCST(this);
    }
}

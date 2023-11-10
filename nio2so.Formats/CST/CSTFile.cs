namespace nio2so.Formats.CST
{
    public class CSTFile : Dictionary<string, string>, ITSOImportable
    {
        public string FilePath { get; set; }
        public bool Populated { get; internal set; } = false;
        public void Populate() => CSTImporter.PopulateCST(this);
    }
}

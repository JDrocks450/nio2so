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
        public string? FilePath { get; set; }
        public string? FriendlyName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FilePath)) return default;
                string name = Path.GetFileNameWithoutExtension(FilePath);
                return new string(name.Where(x => char.IsLetter(x)).ToArray());
            }
        }
        public bool Populated { get; internal set; } = false;
        public void Populate() => CSTImporter.PopulateCST(this);
    }
}

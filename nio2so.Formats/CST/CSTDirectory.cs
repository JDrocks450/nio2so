namespace nio2so.Formats.CST
{
    public class CSTDirectory : Dictionary<uint, CSTFile>
    {
        public string[] FileNames { get; internal set; } = Array.Empty<string>();
    }
}

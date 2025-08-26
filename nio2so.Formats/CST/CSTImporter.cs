namespace nio2so.Formats.CST
{
    public class CSTImporter : TSOFileImporterBase<CSTFile>
    {
        public static CSTDirectory ImportDirectory(string Directory)
        {
            if (!System.IO.Directory.Exists(Directory))
                Directory = Path.Combine(Path.GetDirectoryName(Directory),Path.GetFileNameWithoutExtension(Directory));
            if (System.IO.Directory.Exists(Path.Combine(Directory,"english.dir")))
                Directory = Path.Combine(Directory, "english.dir");
            CSTDirectory retVal = new();
            DirectoryInfo info = new DirectoryInfo(Directory);
            if (!info.Exists) throw new DirectoryNotFoundException(Directory);
            uint noId = 9000;
            foreach (var file in info.GetFiles())
            {
                uint id = 0;
                if (!file.Name.Contains('_'))
                    id = noId++;
                else
                {
                    string numberStr = "";
                    foreach (char c in file.Name)
                        if (char.IsDigit(c)) numberStr += c;
                    id = uint.Parse(numberStr);
                }
                retVal.Add(id, new CSTFile() { FilePath = file.FullName });
            }
            return retVal;
        }
        public static CSTFile Import(string FilePath) => new CSTImporter().ImportFromFile(FilePath);
        public override CSTFile Import(Stream stream)
        {
            CSTFile file = new();
            PopulateCST(file, stream);
            return file;
        }

        internal static void PopulateCST(CSTFile file, Stream stream)
        {
            byte SafeReadOne(bool throwEx = true)
            {
                int value = stream.ReadByte();
                if (value < 0)
                {
                    if (throwEx) throw new OverflowException("Attempted to read past the edge of the file!!");
                    else return 0;
                }
                return (byte)value;
            }
            byte SafeReadIgnore(params byte[] Value)
            {
                byte v = 0;
                do
                {
                    v = SafeReadOne(false);
                }
                while (Value.Contains(v));
                return v;
            }
            string ReadKeyString(out bool ignore)
            {
                string returnValue = "";
                ignore = false;
                while (stream.Position < stream.Length)
                {
                    char character = (char)SafeReadOne();
                    if (character == '/')
                        ignore = true;
                    //if (character == 0x20)
                      //  continue;
                    if (character == '^')
                    {
                        stream.Seek(-1, SeekOrigin.Current);
                        break;
                    }
                    if (character == 0x0D)
                    {
                        ignore = true;
                        break;
                    }
                    returnValue += character;
                }
                return returnValue.Trim();
            }
            string ReadValueString()
            {
                string returnValue = "";
                while (stream.Position < stream.Length)
                {
                    char character = (char)SafeReadOne();
                    if (character == '^') break;
                    returnValue += character;
                }
                return returnValue;
            }
            while (stream.Position < stream.Length - 1)
            {
                string Key = ReadKeyString(out bool comment);
                if (comment)// this is a comment, try to apply it to the last value read, if doable
                {
                    KeyValuePair<string, CSTValue> cstValue = file.ElementAtOrDefault(file.Count - 1);
                    if (cstValue.Value != default)
                        cstValue.Value.Comment = Key;
                    goto skip;
                }
                char next = (char)SafeReadIgnore(0x20, 0x0A, 0x0D);
                if (next == '\0')
                    goto skip;
                if (next != '^')
                    throw new FormatException($"This CST file isn't formatted correctly. Expected: ^ Got: {next}");
                string value = ReadValueString();
                file.Add(string.IsNullOrWhiteSpace(Key) ? file.Keys.Count.ToString() : Key, new(value));
            skip:
                byte discard = SafeReadIgnore(0x20, 0x0A, 0x0D);
                if (discard == 0x0) break;
                stream.Seek(-1, SeekOrigin.Current);
            }

            file.Populated = true;
        }

        internal static void PopulateCST(CSTFile file)
        {
            using (Stream stream = File.OpenRead(file.FilePath)) PopulateCST(file, stream);
        }
    }
}

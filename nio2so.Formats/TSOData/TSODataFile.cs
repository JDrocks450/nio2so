using System.Dynamic;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Xml.Linq;

namespace nio2so.Formats.TSOData
{
    public interface ITSODataStringIDAccessor
    {
        TSODataString GetStringByID(uint ID);
        uint GetIDByString(string Value);
    }

    /// <summary>
    /// Check out the file structure here: <see href="http://wiki.niotso.org/TSOData"/>
    /// </summary>
    public class TSODataFile : ITSOImportable, ITSODataStringIDAccessor
    {
        [Flags]
        public enum Levels
        {
            One = 1 << 0, // 1
            Two = 1 << 1, // 2
            Derived = 1 << 2, // 4

            All = One | Two | Derived
        }

        public DateTime TimeStamp { get; set; }
        public uint LevelOneStructsCount => (uint)LevelOneStructs.Count;
        public List<TSODataStruct> LevelOneStructs { get; } = new();
        public uint LevelTwoStructsCount => (uint)LevelTwoStructs.Count;
        public List<TSODataStruct> LevelTwoStructs { get; } = new();
        public uint DerivedStructsCount => (uint)DerivedStructs.Count;
        public List<TSODerivedStruct> DerivedStructs { get; } = new();
        public uint StringsCount => (uint)Strings.Count;
        public Dictionary<uint,TSODataString> Strings { get; } = new();

        public object GetTypeByString(string TypeName)
        {
            object getDefault(Type type) => Activator.CreateInstance(type);

            switch (TypeName)
            {
                case "bool":
                    return getDefault(typeof(bool));

                case "Uint8":
                    return getDefault(typeof(byte));

                case "Uint16":
                    return getDefault(typeof(ushort));

                case "Uint32":
                    return getDefault(typeof(uint));

                case "Uint64":
                    return getDefault(typeof(ulong));

                case "Sint8":
                    return getDefault(typeof(sbyte));

                case "Sint16":
                    return getDefault(typeof(short));

                case "Sint32":
                    return getDefault(typeof(int));

                case "Sint64":
                    return getDefault(typeof(long));

                case "string":
                    return "";

                case "iunknown":
                    return getDefault(typeof(byte[]));  

                default:
                    break;
            }

            //**try level one struct 
            return GetObject(TypeName, out _);

            throw new ArgumentException($"Unknown type: {TypeName}");
        }

        public object GetObject(uint StringID, out ITSODataStruct TSODataStruct)
        {
            string StructName = Strings[StringID].Value;
            return GetObject(StructName, out TSODataStruct);
        }

        public object GetObject(string StringName, out ITSODataStruct TSODataStruct)
        {
            if (!TryGetStructByName(StringName, out TSODataStruct, Levels.All))
                throw new ArgumentException("Struct matching name not found", nameof(StringName));
            if (TSODataStruct is TSODerivedStruct derived)
                return getDerivedObject(derived);
            else if (TSODataStruct is TSODataStruct data)
                return getStructObject(data);
            throw new FileNotFoundException("Struct not found by name.");
        }

        private object getStructObject(TSODataStruct data)
        {
            ExpandoObject obj = new ExpandoObject();
            for(int i =0; i < data.Fields.Count; i++)
            {
                var field = data.Fields[i];
                string TypeDescriptor = field.TypeString;
                object fieldValue = GetTypeByString(TypeDescriptor);
                if (fieldValue is null)
                    throw new InvalidOperationException($"Failed to get type for field {field.NameString} of type {TypeDescriptor}");
                switch (field.Classification)
                {
                    case TSODataFieldClassification.SingleField:
                        break;
                    case TSODataFieldClassification.TypedList:
                        fieldValue = Array.CreateInstance(fieldValue.GetType(), 0);
                        break;
                    case TSODataFieldClassification.Map:
                        throw new NotImplementedException("Map field classification is not implemented yet.");
                        fieldValue = new Dictionary<string, object>();
                        break;
                }
                obj.TryAdd(field.NameString, fieldValue);
            }   
            return obj;
        }

        private object getDerivedObject(TSODerivedStruct tsoStruct)
        {
            var parent = GetObject(tsoStruct.ParentString, out _);
            if (parent == null)
                throw new InvalidOperationException($"Failed to get parent struct for derived struct {tsoStruct.NameString} with Base Struct {tsoStruct.ParentString}");
            if (parent is not ExpandoObject expandoParent)
                throw new InvalidOperationException($"Parent struct for derived struct {tsoStruct.NameString} is not an {nameof(ExpandoObject)}");
            
            return expandoParent;
        }        

        public bool StringExists(uint StringID) => Strings.ContainsKey(StringID);
        public bool TryGetStringByID(uint ID, out TSODataString str) => Strings.TryGetValue(ID, out str);

        public bool TryGetStructByID(uint ID, out ITSODataStruct Struct, Levels Level = Levels.All)
        {
            Struct = null;
            if (Level.HasFlag(Levels.One))
                Struct = LevelOneStructs.FirstOrDefault(x => x.NameID == ID);
            if (Struct != null)
                return true;
            if (Level.HasFlag(Levels.Two))
                Struct = LevelTwoStructs.FirstOrDefault(x => x.NameID == ID);
            if (Struct != null)
                return true;
            if (Level.HasFlag(Levels.Derived))
                Struct = DerivedStructs.FirstOrDefault(x => x.NameID == ID);
            return Struct != null;
        }

        public bool TryGetStructByName(string Name, out ITSODataStruct Struct, Levels Level = Levels.All)
        {
            Struct = null;
            if (Level.HasFlag(Levels.One))
                Struct = LevelOneStructs.FirstOrDefault(x => x.NameString == Name);
            if (Struct != null)
                return true;
            if (Level.HasFlag(Levels.Two))
                Struct = LevelTwoStructs.FirstOrDefault(x => x.NameString == Name);
            if (Struct != null)
                return true;
            if (Level.HasFlag(Levels.Derived))
                Struct = DerivedStructs.FirstOrDefault(x => x.NameString == Name);
            return Struct != null;
        }

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

        public TSODataString GetStringByID(uint ID) => Strings[ID];

        public uint GetIDByString(string Value) => Strings.First(x => x.Value.Value == Value).Key;
    }
}

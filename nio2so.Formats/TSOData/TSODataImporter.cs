using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nio2so.Formats.TSOData
{    
    public abstract class TSODataObject
    {
        protected TSODataFile ParentFile { get; set; }
        [JsonIgnore]
        public uint NameID { get; set; }
        public string NameString => ParentFile.Strings[NameID].Value;
    }
    public enum TSODataFieldClassification : byte
    {
        SingleField,
        Map,
        TypedList
    }
    public class TSODataField : TSODataObject
    {
        public TSODataField(uint fieldID, TSODataFieldClassification classific, uint typeStrID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = fieldID;
            Classification = classific;
            TypeID = typeStrID;
        }

        public string TypeString => ParentFile.Strings[TypeID].Value;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TSODataFieldClassification Classification { get; set; }
        [JsonIgnore]
        public uint TypeID { get;set; }             
    }
    public class TSODataStruct : TSODataObject
    {
        public TSODataStruct(uint strID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = strID;
        }
        public uint FieldCount => (uint)Fields.Count;
        public List<TSODataField> Fields { get; } = new();
    }

    public enum TSOFieldMaskValues : byte
    {
        None,
        Keep,
        Remove
    }

    public class TSOFieldMask : TSODataObject
    {
        public TSOFieldMask(uint fieldMaskID, TSOFieldMaskValues value)
        {
            ParentFile = TSODataImporter.Current;
            NameID = fieldMaskID;
            Values = value;
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TSOFieldMaskValues Values { get; set; }
    }

    public class TSODerivedStruct : TSODataObject
    {
        public TSODerivedStruct(uint myNameID, uint parentNameID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = myNameID;
            ParentID = parentNameID;
        }

        public string ParentString => ParentFile.Strings[ParentID].Value;
        [JsonIgnore]
        public uint ParentID { get; set; }
        [JsonIgnore]
        public uint FieldMasksCount => (uint)FieldMasks.Count;
        public List<TSOFieldMask> FieldMasks { get; } = new();
    }

    public enum TSODataStringCategories
    {
        None, 
        Field,
        FirstLevel,
        SecondLevel,
        Derived
    }

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

    public class TSODataFile
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
            /*
            return
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
                $"End of file.";*/
        }
    }

    public static class TSODataImporter
    {
        internal static TSODataFile Current { get; private set; }
        public static TSODataFile Import(string FilePath)
        {
            uint readUint(Stream stream)
            {
                byte[] fooArray = new byte[4];
                stream.ReadExactly(fooArray, 0, 4);
                return BitConverter.ToUInt32(fooArray, 0);
            }
            uint readUshort(Stream stream)
            {
                byte[] fooArray = new byte[2];
                stream.ReadExactly(fooArray, 0, 2);
                return BitConverter.ToUInt16(fooArray, 0);
            }
            TSODataStruct getStruct(Stream fs)
            {
                uint strID = readUint(fs);
                uint fieldCount = readUint(fs);
                TSODataStruct currentStruct = new(strID);
                for (int fieldEntry = 0; fieldEntry < fieldCount; fieldEntry++)
                {
                    uint fieldID = readUint(fs);
                    TSODataFieldClassification classific = (TSODataFieldClassification)(byte)fs.ReadByte();
                    uint typeStrID = readUint(fs);
                    TSODataField field = new(fieldID, classific, typeStrID);
                    currentStruct.Fields.Add(field);
                }
                return currentStruct;
            }
            TSODerivedStruct getDerivedStruct(Stream fs)
            {
                uint myNameID = readUint(fs);
                uint parentNameID = readUint(fs);
                uint maskCount = readUint(fs);
                TSODerivedStruct tSODerivedStruct = new(myNameID, parentNameID);
                for (int fieldEntry = 0; fieldEntry < maskCount; fieldEntry++)
                {
                    uint fieldMaskID = readUint(fs);
                    TSOFieldMaskValues value = (TSOFieldMaskValues)(byte)fs.ReadByte();
                    tSODerivedStruct.FieldMasks.Add(new(fieldMaskID, value));
                }
                return tSODerivedStruct;
            }

            TSODataFile file = Current =  new();

            using (FileStream fs = File.OpenRead(FilePath))
            {
                uint UnixTimestamp = readUint(fs);
                file.TimeStamp = DateTime.UnixEpoch.AddSeconds(UnixTimestamp);
                // ** first level structs
                uint entryCount = readUint(fs);
                for(uint structEntry = 0; structEntry < entryCount; structEntry++)
                    file.LevelOneStructs.Add(getStruct(fs));                
                // ** level two structs
                entryCount = readUint(fs);
                for (uint structEntry = 0; structEntry < entryCount; structEntry++)
                    file.LevelTwoStructs.Add(getStruct(fs));
                // ** derived
                entryCount = readUint(fs);
                for (uint structEntry = 0; structEntry < entryCount; structEntry++)
                    file.DerivedStructs.Add(getDerivedStruct(fs));
                //**strings
                entryCount = readUint(fs);
                for (uint structEntry = 0; structEntry < entryCount; structEntry++)
                {
                    uint strId = readUint(fs);
                    string value = "";
                    do
                    {
                        byte b = (byte)fs.ReadByte();
                        char c = Encoding.UTF8.GetString(new byte[] { b })[0];
                        if (c == '\0') break;
                        value += c;
                    }
                    while (true);
                    TSODataStringCategories category = (TSODataStringCategories)(byte)fs.ReadByte();
                    file.Strings.Add(strId, new(value, category));
                }
            }
            Current = null;
            return file;
        }
    }
}

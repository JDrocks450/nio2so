using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.TSOData
{   
    /// <summary>
    /// Imports a <see cref="TSODataFile"/> such as TSODataDefinition.dat in the root directory of The Sims Online (New and Improved or Pre-Alpha)
    /// <para>These files map out the data structures used by The Sims Online in its networked calls.</para>
    /// <para>In TSO Pre-Alpha, it is pretty desolate with only a few structures actually mapped out.</para>
    /// <para/>Furthermore, it is unclear if it even used at this time in Pre-Alpha.
    /// </summary>
    public class TSODataImporter : TSOFileImporterBase<TSODataFile>
    {
        /// <summary>
        /// Do not use, this is kind of a hack for translating IDs to Strings. Just leave it alone ;)
        /// </summary>
        internal static TSODataFile Current { get; private set; }
        /// <summary>
        /// Imports a <see cref="TSODataFile"/> such as TSODataDefinition.dat in the root directory of The Sims Online.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static TSODataFile Import(string FilePath)
        {
            return new TSODataImporter().ImportFromFile(FilePath);
        }
        /// <summary>
        /// Imports a <see cref="TSODataFile"/> such as TSODataDefinition.dat in the root directory of The Sims Online from a stream.
        /// </summary>
        /// <returns></returns>
        public override TSODataFile Import(Stream stream)
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

            TSODataFile file = Current = new();

            var fs = stream;
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

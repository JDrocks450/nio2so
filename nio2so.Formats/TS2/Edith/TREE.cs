using nio2so.Formats.ARCHIVE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Formats.TS2.Edith.TREE;

namespace nio2so.Formats.TS2.Edith
{
    public class TREE : List<TREENodeEntry>
    {
        public enum TREEBoxType : ushort
        {
            Primitive = 0,
            True = 1,
            False = 2,
            Comment = 3,
            Label = 4,
            Goto = 5 //no comment size, roughly primitive sized (180, 48), pointer goes to Label
        }

        private const string TREE_MAGIC = "EERT";

        public uint Version { get; private set; }

        /// <summary>
        /// Compatible with the Sims 2 TREE chunk in Package files.
        /// </summary>
        public class TREENodeEntry
        {
            public TREEBoxType Type { get; set; }
            public ushort Unknown { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public ushort X { get; set; }
            public ushort Y { get; set; }
            public ushort TrueConnection { get; set; }
            public ushort Special { get; set; }            
            public ushort ErrorConnection { get; set; }
            public uint FalseConnection { get; set; } = 0xFFFFFFFF;
            /// <summary>
            /// 1 byte prefix
            /// </summary>
            public byte CommentLength { get; set; }
            /// <summary>
            /// 1 byte length prefix
            /// </summary>
            public String Comment { get; set; } = "";
            public uint TrailingZero { get; set; }

            public void Read(Stream Stream)
            {
                BinaryReader reader = new BinaryReader(Stream); // do not close base stream thats handled by the caller                
                Type = (TREEBoxType)reader.ReadUInt16();
                Unknown = reader.ReadUInt16();
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                X = reader.ReadUInt16();
                Y = reader.ReadUInt16();
                ErrorConnection = reader.ReadUInt16();
                TrueConnection = reader.ReadUInt16();                
                Special = reader.ReadUInt16();                
                FalseConnection = reader.ReadUInt32();
                CommentLength = reader.ReadByte();
                if (CommentLength > 0)
                    Comment = new String(reader.ReadChars(CommentLength));
                TrailingZero = reader.ReadUInt32();
                if (TrailingZero != 0)
                    throw new InvalidDataException($"Trailing Zero is not 0. Its: {TrailingZero}");
            }

            public static TREENodeEntry ReadFromStream(Stream stream)
            {
                TREENodeEntry entry = new();
                entry.Read(stream);
                return entry;
            }

            public override string ToString()
            {
                return $"{nameof(TREENodeEntry)} Type: {Type} Tr: {TrueConnection:X} F: {FalseConnection:X} S: {Special:X} Unk: {Unknown:X} Unk2: {FalseConnection:X} \n{Comment}";
            }
        }

        public TREE()
        {

        }

        public void Read(Stream Stream)
        {
            Stream.Seek(0x44, SeekOrigin.Current);

            BinaryReader reader = new BinaryReader(Stream); // do not close base stream thats handled by the caller
            Version = reader.ReadUInt32();
            string magic = new string(reader.ReadChars(4));
            if (magic != TREE_MAGIC)
                throw new InvalidDataException($"MAGIC must be {TREE_MAGIC}, this is: {magic}.");
            const int HEADER_LEN = 0x18;

            reader.BaseStream.Seek(HEADER_LEN, SeekOrigin.Current);

            while(reader.BaseStream.Position < reader.BaseStream.Length)            
                Add(TREENodeEntry.ReadFromStream(reader.BaseStream));            
        }
    }
}

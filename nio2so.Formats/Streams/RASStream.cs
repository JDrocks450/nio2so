#define NIO2SO

using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Util.Endian;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using static nio2so.Formats.Streams.RASStream;

namespace nio2so.Formats.Streams
{
    /// <summary>
    /// A data structure implementation compatible with RAS in The Sims Online: Pre-Alpha
    /// </summary>
    public class RASStream
    {
        public const uint RASHeader_LittleEndian = 0x5F534152; // _SAR

        public interface IChunkName
        {
            public Endianness Endianness { get; }
            public uint ChunkType { get; }
            /// <summary>
            /// Gets the friendly name of the ChunkType by viewing it as a 4-character code. 
            /// <para/>
            /// For example, if the ChunkType is 0x544F435F, this will return "TOC_". 
            /// The endianness of the chunk type is determined by the <see cref="endian"/> field, which is set when reading the chunk from the stream.
            /// </summary>
            public string ChunkName
            {
                get
                {
                    EndianBitConverter dataConverter = Endianness == Endianness.BigEndian
                        ? EndianBitConverter.Big : EndianBitConverter.Little;

                    byte[] bytes = dataConverter.GetBytes(ChunkType);
                    return Encoding.UTF8.GetString(bytes).ToUpper();
                }
            }
        }

        public class RASHeader
        {
            public const uint LENGTH = sizeof(uint) * 3; //12 bytes

            /// <summary>
            /// "_SAR" header (RAS_) in little-endian format <see cref="RASHeader_LittleEndian"/>
            /// </summary>
            public uint RAS { get; set; } = RASHeader_LittleEndian;
            /// <summary>
            /// Effectively the entire RASStream length, plus an additional 8 bytes.
            /// <para/><b>For Pre-Alpha SetHouseBlob PDU:</b><para/>
            /// This is the <see cref="DecompressedSize"/> + the length of all bytes from HouseID (inclusive) to this field (inclusive) (20 bytes)
            /// </summary>
#if NIO2SO
            [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
#endif
            public uint TotalSize { get; set; } = 0;
            /// <summary>
            /// Seems to be 0x02 as a uint -- unsure. Could be version? Could be chunks? Both numbers are maybe 2?
            /// </summary>
#if NIO2SO
            [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
#endif
            public ushort Version { get; set; } = 0;
            public ushort Unknown { get; set; } = 0;



            public override string ToString() => string.Join(" ", GetDescriptionStrings());

            public string[] GetDescriptionStrings() =>
            [
                $"{nameof(RAS)}: {RAS:X8}",
                $"{nameof(Unknown)}: {Unknown}",
                $"{nameof(TotalSize)} (Reported in Header): {TotalSize}",
                $"{nameof(Version)}: {Version}"
            ];
        }

        /// <summary>
        /// Inventory of all chunks that are expected in a <see cref="RASArchive"/> and what index to find each one.
        /// </summary>
        public class RASTableOfContents : IDictionary<uint,RASTableOfContents.RASTOCEntry>, ITSOCustomSerializableType, IChunkName
        {
            public const uint SIZE = sizeof(uint) + sizeof(ushort); // 6
            public const uint TOC_ = 0x544F435F; // TOC_ (big-endian format) // 0x5F434F54 _COT little-endian

            public class RASTOCEntry : IChunkName
            {
                public const int SIZE = sizeof(uint) * 3; //12 bytes
                public RASTOCEntry(uint chunkType, uint unknown, uint fileOffset)
                {
                    ChunkType = chunkType;
                    Unknown = unknown;
                    FileOffset = fileOffset;
                }

                [IgnoreDataMember]
                public Endianness Endianness { get; internal set; } = Endianness.BigEndian;

                public uint ChunkType { get; set; }
                public uint Unknown { get; set; }
                public uint FileOffset { get; set; }

                public override string ToString() => $"Table: {((IChunkName)this).ChunkName} ({Unknown}) [Offset: {FileOffset:X8}]";
            }

            /// <summary>
            /// The TOC_ ChunkType
            /// </summary>
            public uint ChunkType { get; set; } = TOC_;
            /// <summary>
            /// The amount of chunks in the archive, as read from the stream.
            /// </summary>
            public ushort ChunksAmount { get; private set; } = 0;
            /// <summary>
            /// The supposed endianness given by determining the byte order of the TOC_ header.
            /// </summary>
            [IgnoreDataMember] public Endianness Endianness { get; private set; } = Endianness.BigEndian;
            [IgnoreDataMember] public string ChunkName => ((IChunkName)this).ChunkName;
            
            /// <summary>
            /// The list of all expected chunks in the archive, as read from the stream. Maps the ChunkType to the offset where the chunk is located in the stream, as well as an unknown parameter.
            /// </summary>
            private Dictionary<uint, RASTOCEntry> TableContents { get; } = new();
            

            public RASTableOfContents()
            {
                
            }

            /// <summary>
            /// For non-nio2so implementations, you can use this function to read a <see cref="RASTableOfContents"/> from a <see cref="Stream"/>.
            /// </summary>
            /// <param name="Stream"></param>
            public void ReadFromStream(Stream Stream) => OnDeserialize(Stream);

            public void OnDeserialize(Stream Stream)
            {
                //little endian
                EndianBitConverter dataConverter = EndianBitConverter.Big;
                Endianness = Endianness.BigEndian;

                uint readUint()
                {
                    byte[] buffer = new byte[4];
                    Stream.ReadExactly(buffer, 0, 4);
                    return dataConverter.ToUInt32(buffer, 0);
                }
                ushort readUshort()
                {
                    byte[] buffer = new byte[2];
                    Stream.ReadExactly(buffer, 0, 2);
                    return dataConverter.ToUInt16(buffer, 0);
                }

                uint toc = readUint();
                
                if (toc != TOC_)
                { // big endian
                    dataConverter = EndianBitConverter.Little;
                    Endianness = Endianness.LittleEndian;
                    Stream.Seek(-sizeof(uint), SeekOrigin.Current);
                    toc = readUint();
                }
                if (toc != TOC_)
                    throw new Exception("Did not find Table of Contents!");

                //amount of chunks
                ChunksAmount = readUshort();

                //read each chunk
                for(int i = 0; i < ChunksAmount; i++)
                {
                    uint chunktype = readUint();
                    uint unk = readUint();
                    uint offset = readUint();
                    TableContents.Add(chunktype,new(chunktype, unk, offset) { Endianness = this.Endianness});
                }
            }

            public byte[] OnSerialize()
            {
                using MemoryStream stream = new();

                ChunksAmount = (ushort)TableContents.Count;

                //make endian
                EndianBitConverter dataConverter = Endianness == Endianness.BigEndian
                    ? EndianBitConverter.Big : EndianBitConverter.Little;

                stream.Write(dataConverter.GetBytes(ChunkType), 0, 4);
                stream.Write(dataConverter.GetBytes(ChunksAmount), 0, 2);

                for(int i = 0; i < ChunksAmount; i++)
                {
                    var entry = TableContents.ElementAt(i).Value;
                    stream.Write(dataConverter.GetBytes(entry.ChunkType), 0, 4);
                    stream.Write(dataConverter.GetBytes(entry.Unknown), 0, 4);
                    stream.Write(dataConverter.GetBytes(entry.FileOffset), 0, 4);
                }

                return stream.ToArray();
            }

            #region IDictionary Implementation
            public ICollection<uint> Keys => ((IDictionary<uint, RASTOCEntry>)TableContents).Keys;

            public ICollection<RASTOCEntry> Values => ((IDictionary<uint, RASTOCEntry>)TableContents).Values;

            public int Count => ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).Count;

            public bool IsReadOnly => ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).IsReadOnly;

            public RASTOCEntry this[uint key] { get => ((IDictionary<uint, RASTOCEntry>)TableContents)[key]; set => ((IDictionary<uint, RASTOCEntry>)TableContents)[key] = value; }
            
            public void Add(uint key, RASTOCEntry value)
            {
                ((IDictionary<uint, RASTOCEntry>)TableContents).Add(key, value);
            }

            public bool ContainsKey(uint key)
            {
                return ((IDictionary<uint, RASTOCEntry>)TableContents).ContainsKey(key);
            }

            public bool Remove(uint key)
            {
                return ((IDictionary<uint, RASTOCEntry>)TableContents).Remove(key);
            }

            public bool TryGetValue(uint key, [MaybeNullWhen(false)] out RASTOCEntry value)
            {
                return ((IDictionary<uint, RASTOCEntry>)TableContents).TryGetValue(key, out value);
            }

            public void Add(KeyValuePair<uint, RASTOCEntry> item)
            {
                ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).Add(item);
            }

            public void Clear()
            {
                ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).Clear();
            }

            public bool Contains(KeyValuePair<uint, RASTOCEntry> item)
            {
                return ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).Contains(item);
            }

            public void CopyTo(KeyValuePair<uint, RASTOCEntry>[] array, int arrayIndex)
            {
                ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).CopyTo(array, arrayIndex);
            }

            public bool Remove(KeyValuePair<uint, RASTOCEntry> item)
            {
                return ((ICollection<KeyValuePair<uint, RASTOCEntry>>)TableContents).Remove(item);
            }

            public IEnumerator<KeyValuePair<uint, RASTOCEntry>> GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<uint, RASTOCEntry>>)TableContents).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)TableContents).GetEnumerator();
            }
            #endregion
        }

        /// <summary>
        /// An IFF-like chunk structure, where each chunk is defined by a type, a parameter, a size, and the content itself. 
        /// Has a Table of Contents that defines where each chunk is located in the stream, as well as its type and an unknown parameter. 
        /// The content of the chunk is stored as a byte array.
        /// </summary>
        public class RASArchive : ITSOCustomSerializableType
        {
            /// <summary>
            /// Maps the ChunkType to the chunk data, as read from the stream. See: <see cref="RASChunk"/>
            /// </summary>
            [IgnoreDataMember] public Dictionary<uint, RASChunk> Chunks { get; } = new();
            /// <summary>
            /// <inheritdoc cref="RASTableOfContents"/>
            /// </summary>
            public RASTableOfContents TableOfContents { get; set; } = new();
            /// <summary>
            /// Represents a chunk of formatted data contained in a <see cref="RASArchive"/>. Each chunk has a type, an unknown parameter, a size, and the content itself as a byte array.
            /// </summary>
            /// <param name="ChunkType"></param>
            /// <param name="Param1"></param>
            /// <param name="Size"></param>
            /// <param name="Content"></param>
            public record RASChunk(uint ChunkType, uint Param1, uint Size, byte[] Content)
            {
                internal Endianness endian = Endianness.BigEndian;
                /// <summary>
                /// Creates a new <see cref="RASChunk"/> with the given <paramref name="ChunkType"/> and <paramref name="Content"/>
                /// </summary>
                /// <param name="ChunkType"></param>
                /// <param name="Content"></param>
                /// <param name="Param1"></param>
                public RASChunk(uint ChunkType, byte[] Content, uint Param1 = 0) : this(ChunkType, Param1, (uint)Content.Length, Content) { }

                /// <summary>
                /// Gets the friendly name of the ChunktType by viewing it as a 4-character code. 
                /// <para/>
                /// For example, if the ChunkType is 0x544F435F, this will return "TOC_". 
                /// The endianness of the chunk type is determined by the <see cref="endian"/> field, which is set when reading the chunk from the stream.
                /// </summary>
                [IgnoreDataMember]
                public string ChunkName
                {
                    get
                    {
                        EndianBitConverter dataConverter = endian == Endianness.BigEndian
                            ? EndianBitConverter.Big : EndianBitConverter.Little;

                        byte[] bytes = dataConverter.GetBytes(ChunkType);
                        return Encoding.UTF8.GetString(bytes).ToUpper();
                    }
                }

                public override string ToString() => $"{ChunkName} ({Param1}) [Size: {Size:X8}]";
            }
            
            public RASArchive() { }

            public RASArchive(params RASChunk[] Chunks)
            {
                foreach (var chunk in Chunks)
                    this.Chunks.Add(chunk.ChunkType, chunk);
            }

            public RASChunk? GetChunk(uint ChunkType) => Chunks[ChunkType];

            /// <summary>
            /// For non-nio2so implementations, you can use this function to read a <see cref="RASArchive"/> from a <see cref="Stream"/>. 
            /// <para/>
            /// This will read the <see cref="RASTableOfContents"/> first, then read each chunk based on the offsets defined in the TOC.
            /// </summary>
            /// <param name="Stream"></param>
            public void ReadFromStream(Stream Stream) => OnDeserialize(Stream);            

            private void PopulateChunks(Stream Stream)
            {
                Chunks.Clear();

                EndianBitConverter dataConverter = TableOfContents.Endianness == Endianness.BigEndian
                    ? EndianBitConverter.Big : EndianBitConverter.Little;

                uint readUint()
                {
                    byte[] buffer = new byte[4];
                    Stream.ReadExactly(buffer, 0, 4);
                    return dataConverter.ToUInt32(buffer, 0);
                }

                long index = -1;

                while (Stream.Position != Stream.Length)
                {
                    index = Stream.Position;
                    //read 4 byte chunk type
                    uint header = readUint();
                    //id of the chunk
                    uint chunk_id = readUint();
                    //size of the chunk
                    uint size = readUint();

                    byte[] content = new byte[size];
                    Stream.ReadExactly(content, 0, (int)size);
                    Chunks.Add(header, new RASChunk(header, chunk_id, size, content) { endian = TableOfContents.Endianness });
                }
            }

            private byte[] doSerialize()
            {
                //**write blank toc head
                using MemoryStream stream = new();
                long tocSize = RASTableOfContents.SIZE + (Chunks.Count * RASTableOfContents.RASTOCEntry.SIZE);
                byte[] foo = new byte[tocSize];
                stream.Write(foo, 0, foo.Length);

                //make endian
                EndianBitConverter dataConverter = TableOfContents.Endianness == Endianness.BigEndian
                    ? EndianBitConverter.Big : EndianBitConverter.Little;

                //make toc
                RASTableOfContents toc = new();

                //**write chunks
                foreach (var chunk in Chunks.Values)
                {
                    toc.Add(chunk.ChunkType, 
                        new RASTableOfContents.RASTOCEntry(chunk.ChunkType, 0, (uint)stream.Position) { Endianness = TableOfContents.Endianness });

                    stream.Write(dataConverter.GetBytes(chunk.ChunkType), 0, 4);
                    stream.Write(dataConverter.GetBytes(chunk.Param1), 0, 4);
                    stream.Write(dataConverter.GetBytes(chunk.Size), 0, 4);
                    stream.Write(chunk.Content, 0, chunk.Content.Length);
                }

                stream.Seek(0, SeekOrigin.Begin);

                byte[] tocData = toc.OnSerialize();
                stream.Write(tocData, 0, tocData.Length);

                return stream.ToArray();
            }

            public byte[] ToArray() => OnSerialize();

            public byte[] OnSerialize() => doSerialize();

            public void OnDeserialize(Stream Stream)
            {
                TableOfContents.OnDeserialize(Stream);
                PopulateChunks(Stream);
            }
        }

        public static bool VerifyIntegrity(byte[] RASFileData, byte[] SerializedData)
        {
            byte ComputeAdditionChecksum(byte[] data)
            {
                long longSum = data.Sum(x => (long)x);
                return unchecked((byte)longSum);
            }
            return ComputeAdditionChecksum(RASFileData) == ComputeAdditionChecksum(SerializedData);
        }

        //**** Structure of the RASStream is as follows:
        public RASHeader Header { get; set; } = new();
        public RASArchive Content { get; set; } = new();
    }

    public class CompressedRASStream
    {
        public CompressedRASStream() { }

        public CompressedRASStream(RASArchive Archive, bool CompressBlob, uint PreceedingByteLength) : this()
        {            
            byte[] Data = Archive.ToArray();
            //RAS_ + TotalSize (4 bytes) + PreceedingBytes + CompressedStream (Data.Length bytes)
            Header.TotalSize = 4 + (uint)(Data.Length + PreceedingByteLength);

            if (CompressBlob)
                CompressedStream = TSOSerializableStream.ToCompressedStream(Data);
            else
                CompressedStream = new TSOSerializableStream(0x01, Data, (uint)Data.Length);
        }

        public RASHeader Header { get; set; } = new();
        public TSOSerializableStream CompressedStream { get; set; } = new();

        public uint Length => (uint)(RASHeader.LENGTH + CompressedStream.Length);
    }
}

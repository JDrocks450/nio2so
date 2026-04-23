#define NIO2SO

using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Util.Endian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        public class RASHeader
        {
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
            public uint TotalSize { get; set; }
            /// <summary>
            /// Seems to be 0x02 as a uint -- unsure. Could be version? Could be chunks? Both numbers are maybe 2?
            /// </summary>
#if NIO2SO
            [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
#endif
            public ushort Version { get; set; }
            public ushort Unknown { get; set; }
        }

        public class RASTableOfContents : ITSOCustomSerializableType
        {
            public const uint TOC_ = 0x544F435F; // TOC_ (big-endian format)

            public record RASTOCEntry(uint ChunkType, uint Unknown, uint FileOffset);

            public uint ChunkType { get; } = TOC_;
            public ushort ChunksAmount { get; private set; } = 0;
            public Endianness Endianness { get; private set; } = Endianness.BigEndian;
            public Dictionary<uint, RASTOCEntry> TableContents { get; } = new();

            public RASTableOfContents()
            {
                
            }

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
                    TableContents.Add(chunktype,new(chunktype,unk,offset));
                }
            }

            public byte[] OnSerialize()
            {
                throw new NotImplementedException();
            }
        }

        public class RASContent : ITSOCustomSerializableType
        {
            /// <summary>
            /// Maps read chunk to the index it is found at in the source data stream
            /// </summary>
            [IgnoreDataMember] public Dictionary<RASChunk, long> Chunks { get; } = new();

            public RASTableOfContents TableOfContents { get; set; } = new();

            public record RASChunk(uint ChunkType, uint Param1, uint Size, byte[] Content)
            {
                internal Endianness endian = Endianness.BigEndian;

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

                public override string ToString() => $"{ChunkName}({Param1})[{Size}]";
            }
            
            public RASContent() { }

            public RASChunk? GetChunk(uint ChunkType) => Chunks.Keys.LastOrDefault(x => x.ChunkType == ChunkType);

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
                ushort readUshort()
                {
                    byte[] buffer = new byte[2];
                    Stream.ReadExactly(buffer, 0, 2);
                    return dataConverter.ToUInt16(buffer, 0);
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
                    Chunks.Add(new RASChunk(header, chunk_id, size, content) { endian = TableOfContents.Endianness }, index);
                }
            }

            public byte[] OnSerialize()
            {
                throw new NotImplementedException();
            }

            public void OnDeserialize(Stream Stream)
            {
                TableOfContents.OnDeserialize(Stream);
                PopulateChunks(Stream);
            }
        }

        public RASHeader Header { get; set; } = new();
        public RASContent Content { get; set; } = new();
    }

    public class CompressedRASStream
    {
        public RASHeader Header { get; set; } = new();
        public TSOSerializableStream CompressedStream { get; set; } = new();
    }
}

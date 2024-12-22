using nio2so.Formats.FAR3;
using nio2so.Formats.Util.Endian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Formats.Streams
{
    /// <summary>
    /// A TSOSerializableStream that contains a RefPack bitstream
    /// <para/>Sample Header: <c>01 000000AE 000000A9 ...</c>
    /// <para/>Which is Endian (0x01), Decompressed Size, Compressed Size
    /// <para/>This is usually immediately followed by Compressed Size again in a different
    /// Endian then 10 FB for the RefPack magic number.
    /// </summary>
    public class TSOSerializableStream
    {
        const uint TSOSERIALIZABLESTREAM_HEAD_LEN = sizeof(uint) * 2 + 1;

        private MemoryStream _stream;
        
        public byte CompressionEndian { get; set; }
        /// <summary>
        /// Do not trust this number, it's Endian can be literally either one for seemingly no reason
        /// </summary>
        [TSOVoltronValue(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.LittleEndian)]
        /// <summary>
        /// Do not trust this number, it's Endian can be literally either one for seemingly no reason
        /// </summary>
        public uint DecompressedSize { get; set; }
        [TSOVoltronValue(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.LittleEndian)]
        public uint CompressedSize { get; set; }
        [TSOVoltronBodyArray]
        public byte[] StreamContents
        {
            get => ToArray();
            set
            {
                _stream?.Dispose();
                _stream = new MemoryStream();
                _stream.Write(value);
            }
        }

        [IgnoreDataMember]
        public bool CanRead => _stream.CanRead;
        [IgnoreDataMember]
        public bool CanSeek => _stream.CanSeek;
        [IgnoreDataMember]
        public bool CanWrite => _stream.CanWrite;
        [IgnoreDataMember]
        public long Length => _stream.Length;
        [IgnoreDataMember]
        public long Position { get => _stream.Position; set => _stream.Position = value; }

        public TSOSerializableStream() : base()
        {
            _stream = new MemoryStream();
        }
        public TSOSerializableStream(byte Endian, byte[] CompressedRefPack, uint DecompressedSize) : this()
        {
            this.DecompressedSize = DecompressedSize;
            CompressedSize = (uint)CompressedRefPack.Length;
            CompressionEndian = Endian;            
            _stream.Write(CompressedRefPack);
        }

        public byte[] ToArray() => _stream.ToArray();

        /// <summary>
        /// If this <see cref="TSOSerializableStream"/> contains a RefPack bitstream, you can
        /// use this function to decompress the bitstream.
        /// </summary>
        /// <returns></returns>
        public byte[] DecompressRefPack()
        {
            int startOffset = 4;
            Seek(startOffset, SeekOrigin.Begin);
            byte[] datastream = new byte[Length - startOffset];
            Read(datastream, 0, datastream.Length);
            byte[] fileData = new Decompresser().Decompress(datastream);            
            return fileData;
        }

        /// <summary>
        /// Compresses the incoming <paramref name="DecompressedBytes"/> to a RefPack stream
        /// and creates a new <see cref="TSOSerializableStream"/> from the compressed bytes.
        /// </summary>
        /// <returns></returns>
        public static TSOSerializableStream ToCompressedStream(byte[] DecompressedBytes)
        {
            Decompresser compresser = new();
            byte[] compressedBytes = compresser.Compress(DecompressedBytes, true); // ensure the data length is wrote before magic number
            return new TSOSerializableStream(0x01, compressedBytes, (uint)DecompressedBytes.Length);
        }

        /// <summary>
        /// Reads a new <see cref="TSOSerializableStream"/> from the given <see cref="Stream"/>
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        [Obsolete] public static TSOSerializableStream FromStream(Stream Data)
        {            
            long startPosition = Data.Position;
            byte bodyType = (byte)Data.ReadByte();                        
            Endianness endian = bodyType switch
            {
                0x00 => Endianness.BigEndian,
                0x01 => Endianness.LittleEndian,
                0x02 => Endianness.LittleEndian,
                0x03 => Endianness.BigEndian,
            };
            //decompressed size
            uint read_length = 0;
            uint size = read_length = bodyType == 0x02 ? ReadReverseDword(Data, endian) : ReadDword(Data, endian);
            long offset = 0;
            bool hasCompression = false;
            if (bodyType == 0x00) // no compression
                offset = Data.Position - startPosition;
            else // has compression
            {
                hasCompression = true;
                uint compressed_size = ReadDword(Data, endian);                
                read_length = compressed_size;                
            }
            byte[] payload = new byte[read_length];
            Data.ReadExactly(payload, 0, payload.Length);
            return new TSOSerializableStream(bodyType, payload, size)
            {
                CompressedSize = hasCompression ? read_length : 0
            };
        }  

        static uint ReadDword(Stream Data, Endianness Endian)
        {
            byte[] dataBytes = new byte[4];
            Data.Read(dataBytes, 0, 4);
            if (Endian == Endianness.BigEndian)
                return EndianBitConverter.Big.ToUInt32(dataBytes,0);
            return EndianBitConverter.Little.ToUInt32(dataBytes, 0);
        }
        static uint ReadReverseDword(Stream Data, Endianness Endian)
        {
            byte[] dataBytes = new byte[4];
            Data.Read(dataBytes, 0, 4);
            Array.Reverse(dataBytes);
            if (Endian == Endianness.BigEndian)
                return EndianBitConverter.Big.ToUInt32(dataBytes, 0);
            return EndianBitConverter.Little.ToUInt32(dataBytes, 0);
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public uint GetTotalLength() => (uint)(TSOSERIALIZABLESTREAM_HEAD_LEN + Length);

        public void FlipEndian()
        {
            uint flip(uint number)
            {
                byte[] array = EndianBitConverter.Little.GetBytes(number);
                return EndianBitConverter.Big.ToUInt32(array, 0);
            }
            CompressedSize = flip(CompressedSize);
            DecompressedSize = flip(DecompressedSize);
        }

        public override string ToString() => 
            $"{nameof(TSOSerializableStream)}(Decompressed: {DecompressedSize}, Compressed: {CompressedSize})[{Length}]";
    }
}

using nio2so.Formats.FAR3;
using nio2so.Formats.Util.Endian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.Streams
{
    public class TSOSerializableStream : Stream
    {
        private MemoryStream _stream;

        public uint BodyType { get; set; }
        public uint DecompressedSize { get; set; }
        public uint CompressedSize { get; set; }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public TSOSerializableStream(byte BodyType, byte[] Payload)
        {
            this.BodyType = BodyType;
            _stream = new MemoryStream();
            Write(Payload);
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
            byte[] fileData = new Decompresser()
            {
                DecompressedSize = DecompressedSize
            }.Decompress(datastream);
            return fileData;
        }

        /// <summary>
        /// Reads a new <see cref="TSOSerializableStream"/> from the given <see cref="Stream"/>
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static TSOSerializableStream FromStream(Stream Data)
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
            return new TSOSerializableStream(bodyType, payload)
            {
                DecompressedSize = size,
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

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}

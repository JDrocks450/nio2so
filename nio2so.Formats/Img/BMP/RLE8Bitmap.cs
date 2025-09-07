using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.Img.BMP
{
    public class RLE8Bitmap
    {
        private enum Compression
        {
            // others not necessary for the 8bpp compression, but left for reference
            //BI_RGB = 0x0000,
            BI_RLE8 = 0x0001,
            //BI_RLE4 = 0x0002,
            //BI_BITFIELDS = 0x0003,
            //BI_JPEG = 0x0004,
            //BI_PNG = 0x0005,
            //BI_CMYK = 0x000B,
            //BI_CMYKRLE8 = 0x000C,
            //BI_CMYKRLE4 = 0x000D
        }

        private enum BitCount
        {
            // others not necessary for the 8bpp compression, but left for reference
            //Undefined = (ushort)0x0000,
            //TwoColors = (ushort)0x0001,
            //Max16Colors = (ushort)0x0004,
            Max256Colors = (ushort)0x0008,
            //Max32KBColors = (ushort)0x0010,
            //Max16MBColors = (ushort)0x0018,
            //Max16MBColors_Compressed = (ushort)0x0020
        }

        private struct RleCompressedBmpHeader
        {
            // Everything before the HeaderSize is technically not part of the header (it's not included in the HeaderSize calculation)

            /// <summary>
            /// Size of the .bmp file.
            /// Always header size (40), plus palette size, plus image size, plus pre-header size (14);
            /// </summary>
            public uint Size;

            /// <summary>
            /// Offset to start of image data in bytes from the start of the file
            /// </summary>
            public uint Offset;

            /// <summary>
            /// Size of this header in bytes. (Always 40)
            /// </summary>
            public uint HeaderSize; // 4 + 4 + 4 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4

            /// <summary>
            /// Width of bitmap in pixels
            /// </summary>
            public int Width;

            /// <summary>
            /// Height of bitmap in pixels
            /// </summary>
            public int Height;

            /// <summary>
            /// Number of Planes (layers). Always 1.
            /// </summary>
            public ushort Planes;
            /// <summary>
            /// Number of bits that define each pixel and maximum number of colors
            /// </summary>
            public BitCount BitCount;

            /// <summary>
            /// Defines the compression mode of the bitmap.
            /// </summary>
            public Compression Compression;

            /// <summary>
            /// Size, in bytes, of image.
            /// </summary>
            public uint ImageSize;

            // These shouldn't really be all that important
            public uint XPixelsPerMeter;
            public uint YPixelsPerMeter;

            /// <summary>
            /// The number of indexes in the color table used by this bitmap.
            /// <para>0 - Use max available</para>
            /// <para>If BitCount is less than 16, this is the number of colors used by the bitmap</para>
            /// <para>If BitCount is 16 or greater, this specifies the size of the color table used to optimize performance of the system palette.</para>
            /// </summary>
            public uint ColorUsed;

            /// <summary>
            /// Number of color indexes that are required for displaying the bitmap. 0 means all color indexes are required.
            /// </summary>
            public uint ColorImportant;

            public byte[] ToBytes()
            {
                var swap = BitConverter.IsLittleEndian;
                var result = new List<byte>();

                result.AddRange(new byte[] { 0x42, 0x4d }); // signature (BM)
                result.AddRange(BitConverter.GetBytes(Size));
                result.AddRange(new byte[4]); // reserved
                result.AddRange(BitConverter.GetBytes(Offset));
                result.AddRange(BitConverter.GetBytes(HeaderSize));
                result.AddRange(BitConverter.GetBytes(Width));
                result.AddRange(BitConverter.GetBytes(Height));
                result.AddRange(BitConverter.GetBytes(Planes));
                result.AddRange(BitConverter.GetBytes((ushort)BitCount));
                result.AddRange(BitConverter.GetBytes((uint)Compression));
                result.AddRange(BitConverter.GetBytes(ImageSize));
                result.AddRange(BitConverter.GetBytes(XPixelsPerMeter));
                result.AddRange(BitConverter.GetBytes(YPixelsPerMeter));
                result.AddRange(BitConverter.GetBytes(ColorUsed));
                result.AddRange(BitConverter.GetBytes(ColorImportant));

                return result.ToArray();
            }
        }

        public static unsafe byte[] RunLengthEncodeBitmap(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format8bppIndexed) { throw new ArgumentException("The image must be in 8bppIndexed PixelFormat", "bmp"); }

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            List<byte> result = new List<byte>();
            
            // Actual RLE algorithm. Bottom of image is first stored row, so start from bottom.
            for (var rowIndex = bmp.Height - 1; rowIndex >= 0; rowIndex--)
            {
                byte? storedPixel = null;
                var curPixelRepititions = 0;
                var imageRow = (byte*)data.Scan0.ToPointer() + (rowIndex * data.Stride);
                for (var pixelIndex = 0; pixelIndex < bmp.Width; pixelIndex++)
                {
                    var curPixel = imageRow[pixelIndex];
                    if (!storedPixel.HasValue)
                    {
                        curPixelRepititions = 1;
                        storedPixel = curPixel;
                    }
                    else if (storedPixel.Value != curPixel || curPixelRepititions == 255)
                    {
                        result.Add(Convert.ToByte(curPixelRepititions));
                        result.Add(storedPixel.Value);
                        curPixelRepititions = 1;
                        storedPixel = curPixel;
                    }
                    else
                    {
                        curPixelRepititions++;
                    }
                }

                if (curPixelRepititions > 0)
                {
                    result.Add(Convert.ToByte(curPixelRepititions));
                    result.Add(storedPixel.Value);
                }

                if (rowIndex == 0)
                {
                    // EOF flag
                    result.Add(0x00);
                    result.Add(0x01);
                }
                else
                {
                    // End of Line Flag
                    result.Add(0x00);
                    result.Add(0x00);
                }
            }

            bmp.UnlockBits(data);

            var paletteSize = (uint)bmp.Palette.Entries.Length * 4;
            var header = new RleCompressedBmpHeader();
            header.HeaderSize = 40;
            header.Size = header.HeaderSize + paletteSize + (uint)result.Count + 14;
            header.Offset = header.HeaderSize + 14 + paletteSize; // total header size + palette size
            header.Width = bmp.Width;
            header.Height = bmp.Height;
            header.Planes = 1;
            header.BitCount = BitCount.Max256Colors;
            // as far as I can tell, PixelsPerMeter are not terribly important
            header.XPixelsPerMeter = 0x10000000;
            header.YPixelsPerMeter = 0x10000000;
            header.Compression = Compression.BI_RLE8;
            header.ColorUsed = 256;
            header.ColorImportant = 0; // use all available colors
            header.ImageSize = header.HeaderSize + (uint)result.Count;

            var headerBytes = header.ToBytes();
            var paletteBytes = ConvertPaletteToBytes(bmp.Palette);

            return headerBytes.Concat(paletteBytes).Concat(result).ToArray();
        }

        private static byte[] ConvertPaletteToBytes(ColorPalette colorPalette)
        {
            return colorPalette.Entries.SelectMany(c => new byte[]
                {
                c.B,
                c.G,
                c.R,
                0
                }).ToArray();
        }
    }
}

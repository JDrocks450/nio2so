using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace nio2so.TSOView2.Formats
{
    internal class Bitmap8bpp : IDisposable
    {
        private bool bitsLocked;
        private BitmapData bmd;
        private Bitmap b;

        public int Width => b.Width;
        public int Height => b.Height;
        public ColorPalette ColorPalette => b.Palette;

        /// <summary>
        /// Locks an 8bit image in memory for fast get/set pixel functions.
        /// Remember to Dispose object to release memory.
        /// </summary>
        /// Bitmap reference
        public Bitmap8bpp(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                throw (new System.Exception("Invalid PixelFormat. 8 bit indexed required"));
            b = bitmap; //Store a private reference to the bitmap
            bmd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                             ImageLockMode.ReadWrite, b.PixelFormat);
            bitsLocked = true;
        }
        public Bitmap8bpp(string path) : this(new Bitmap(path))
        {

        }

        public Bitmap8bpp(int width, int height, ColorPalette Palette) : this(new Bitmap(width, height, PixelFormat.Format8bppIndexed)
        {
            Palette = Palette
        })
        {

        }

        /// <summary>

        /// Releases memory
        /// </summary>

        public void Dispose()
        {
            if (bitsLocked)
                b.UnlockBits(bmd);
            bitsLocked = false;
            b.Dispose();
        }

        public void Save(string path)
        {
            if (bitsLocked)
                b.UnlockBits(bmd);
            bitsLocked = false;
            b.Save(path, ImageFormat.Bmp);
        }

        /// <summary>

        /// Gets color of an 8bit-pixel
        /// </summary>

        public unsafe byte GetPixel(int x, int y)
        {
            byte* p = (byte*)bmd.Scan0.ToPointer();
            //always assumes 8 bit per pixels
            int offset = y * bmd.Stride + x;
            return p[offset];
        }

        /// <param name="x">Row</param>
        /// <param name="y">Column</param>
        /// <returns>Color of pixel</returns>
        public unsafe System.Drawing.Color GetPixelColor(int x, int y)
        {
            return GetColorFromIndex(GetPixel(x, y));
        }

        /// <summary>

        /// Sets color of an 8bit-pixel
        /// </summary>

        /// <param name="x">Row</param>
        /// <param name="y">Column</param>
        /// <param name="c">Color index</param>
        public unsafe void SetPixel(int x, int y, byte c)
        {
            byte* p = (byte*)bmd.Scan0.ToPointer();
            //always assumes 8 bit per pixels
            int offset = y * bmd.Stride + (x);
            p[offset] = c;
        }

        /// <summary>

        /// Sets the palette for the referenced image to Grayscale
        /// </summary>

        public void MakeGrayscale()
        {
            SetGrayscalePalette(this.b);
        }

        /// <summary>

        /// Sets the palette of an image to grayscales (0=black, 255=white)
        /// </summary>

        /// <param name="b">Bitmap to set palette on</param>
        public static void SetGrayscalePalette(Bitmap b)
        {
            ColorPalette pal = b.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = Color.FromArgb(255, i, i, i);
            b.Palette = pal;
        }

        private System.Drawing.Color GetColorFromIndex(byte c)
        {
            return b.Palette.Entries[c];
        }
    }
}

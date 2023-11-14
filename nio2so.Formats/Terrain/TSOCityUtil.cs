using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.Terrain
{
    /// <summary>
    /// Adds an iterator to a <see cref="Bitmap"/>
    /// </summary>
    public class UtilImageIndexer<T>
    {
        public UtilImageIndexer(Bitmap ImageReference, Func<Color, T> Converter)
        {
            this.ImageReference = ImageReference;
            this.Converter = Converter;
        }

        public T this[int Index]
        {
            get
            {
                int y = Index / ImageReference.Width;
                int x = Index % ImageReference.Width;
                var color = ImageReference.GetPixel(Math.Min(ImageReference.Width-1, x), Math.Min(ImageReference.Height-1,y));
                return Converter(color);
            }
        }

        public Bitmap ImageReference { get; }
        public Func<Color, T> Converter { get; }
    }

    /// <summary>
    /// Adds an iterator to a <see cref="Bitmap"/>
    /// </summary>
    public class UtilImageIndexer
    {
        public UtilImageIndexer(Bitmap ImageReference)
        {
            this.ImageReference = ImageReference;
        }

        public Color this[int Index]
        {
            get
            {
                int y = Index / ImageReference.Width;
                int x = Index % ImageReference.Width;
                return ImageReference.GetPixel(Math.Min(ImageReference.Width - 1, x), Math.Min(ImageReference.Height - 1, y));
            }
        }

        public Bitmap ImageReference { get; }
    }

    internal static class TSOCityUtil
    {
        /// <summary>
        /// Interates sequentially over each pixel in the image, ignoring the color specified. 
        /// <para>Calls <paramref name="Callback"/> when it finds a pixel that is not <paramref name="IgnoreColor"/></para>
        /// </summary>
        /// <param name="ImgReference"></param>
        /// <param name="IgnoreColor"></param>
        /// <param name="Callback"></param>
        public static void IterateOverImageSequential(ref Bitmap ImgReference, Color IgnoreColor, Action<(Point Pixel, Color Color)> Callback)
        {
            if (ImgReference == null) throw new ArgumentNullException(nameof(ImgReference));
            for (int y = 0; y < ImgReference.Height; y++)
            {
                for (int x = 0; x < ImgReference.Width; x++)
                {
                    var pixel = ImgReference.GetPixel(x, y);
                    //default Equals() doesn't catch this?? 
                    if (pixel.R == IgnoreColor.R && pixel.G == IgnoreColor.G && pixel.B == IgnoreColor.B) continue;
                    Callback((new Point(x, y), pixel));
                }
            }
        }
        /// <summary>
        /// Follows the shape of the TSO Map, ignoring the color specified. 
        /// <para>Calls <paramref name="Callback"/> when it finds a pixel that is not <paramref name="IgnoreColor"/></para>
        /// </summary>
        /// <param name="ImgReference"></param>
        /// <param name="IgnoreColor"></param>
        /// <param name="Callback"></param>
        public static void IterateOverImageMapFunction(TSOCityConstants Settings, ref Bitmap ImgReference, 
            Color? IgnoreColor, Action<(Point Pixel, Point MapPosition, Color Color)> Callback)
        {
            if (ImgReference == null) throw new ArgumentNullException(nameof(ImgReference));
            int row = 0, column = 0, imgX, imgY, originX, originY;

            //will never be zero btw
            originX = Settings.OriginPosition?.X ?? 0;
            originY = Settings.OriginPosition?.Y ?? 0;

            imgX = originX;
            imgY = originY;

            for(int i = 0; i < Settings.CityWidth * Settings.CityHeight; i++)
            { // over each tile in the map
                int fooImgX = imgX;
                int fooImgY = imgY;

                int fooColumn = column;
                int fooRow = row;

                var pixel = Color.White;
                try
                {
                    pixel = ImgReference.GetPixel(imgX, imgY);
                }
                catch(Exception ex)
                { // TSO Pre-Alpha WILL read beyond the line. it is what it is.
                    Debug.WriteLine($"[TSOCityImporter::Elevation] {ex.Message}. (TSO Pre-Alpha?) Set it to {pixel.Name}.");
                }
                //move up one space in the map file
                column++;
                if (column >= Settings.CityWidth)
                { // next row
                    row++;
                    column = 0;
                }
                if (row >= Settings.CityHeight * 2)
                    break;
                //get map position func
                if (row % 2 == 0)
                { // even rows
                    imgX = (originX + row) - column;
                    imgY = (originY + column) + row;
                }
                else
                { // odd rows
                    imgX = (originX + (row - 1)) - column;
                    imgY = ((originY + 1) + column) + (row - 1);
                }
                
                if (IgnoreColor != default)
                {//default Equals() doesn't catch this?? 
                    if (pixel.R == IgnoreColor?.R && pixel.G == IgnoreColor?.G && pixel.B == IgnoreColor?.B)
                        continue;
                }
                Callback((new Point(fooImgX, fooImgY), new Point(fooColumn, fooRow), pixel));
            }
        }
    }
}

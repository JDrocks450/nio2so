using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace nio2so.TSOView2.Util
{
    internal static class WPFInteropExtensions
    {
        /// <summary>
        /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
        /// </summary>
        /// <param name="src">A bitmap image</param>
        /// <returns>The image as a BitmapImage for WPF</returns>
        public static BitmapImage Convert(this Image src, bool TransparentEnabled = false)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, TransparentEnabled ? ImageFormat.Png : ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        public static async Task RenderElement2ClipboardAsync(UIElement Element)
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "clipboard.png");
            using FileStream fs = File.Create(path);
            using Stream s = RenderElement(Element);
            s.Seek(0, SeekOrigin.Begin);
            await s.CopyToAsync(fs);
            var collection = new System.Collections.Specialized.StringCollection
            {
                path
            };
            Clipboard.SetFileDropList(collection);
        }
        public static Stream RenderElement(UIElement Target)
        {
            var element = Target;

            var rect = new Rect(element.RenderSize);
            var visual = new DrawingVisual();

            using (var dc = visual.RenderOpen())
            {
                var brush = new VisualBrush(element)
                {
                    Stretch = Stretch.None
                };
                dc.DrawRectangle(brush, null, rect);
            }

            var bitmap = new RenderTargetBitmap(
                (int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            MemoryStream stm = new MemoryStream();
            {
                encoder.Save(stm);
            }
            return stm;
        }
    }
}

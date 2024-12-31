using Microsoft.Win32;
using nio2so.Formats.Img.Targa;
using nio2so.TSOView2.Util;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace nio2so.TSOView2.Formats.UIs.Subpages
{
    /// <summary>
    /// Interaction logic for ResourceToolWindow.xaml
    /// </summary>
    public partial class ResourceToolWindow : Window
    {
        public ResourceToolWindow()
        {
            InitializeComponent();
        }

        public static void SpawnWithPrompt()
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Title = "Select a resource to open",
                Multiselect = true,
                DereferenceLinks = true,
                CheckFileExists = true,
                CheckPathExists = true,               
            };
            if (dlg.ShowDialog() ?? false)
                foreach (var s in dlg.FileNames)
                    SpawnWithResourceURI(s);
        }

        public static void SpawnWithResourceURI(string URI)
        {            
            BitmapSource? preview = null;
            using var bmp = TargaImage.LoadTargaImage(URI);            
            SpawnWithImageStream(bmp.Convert(true), System.IO.Path.GetFileName(URI));
        }

        public static void SpawnWithImageStream(BitmapSource Source, string Title = "Image")
        {
            ResourceToolWindow wnd = new ResourceToolWindow();
            wnd.ContentImage.Source = Source;
            wnd.Title = Title;
            wnd.Show();
        }
    }
}

using nio2so.TSOView2.Util;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace nio2so.TSOView2.Formats
{
    /// <summary>
    /// Interaction logic for HexDumpWindow.xaml
    /// </summary>
    public partial class HexDumpWindow : Window
    {
        MemoryStream? _openedStream;

        const string MY_DESCRIPTION = "This tool allows you to paste Hex data from " +
            "Edith (ResEdit) to easily format as bytes, save to disk, etc.";

        public HexDumpWindow(Window? owner = default)
        {
            InitializeComponent();

            Owner = owner;
            BlurbMessageLabel.Text = MY_DESCRIPTION;
            ErrorMessageWindow.Visibility = Visibility.Collapsed;
        }

        public void PasteText(string PastedText)
        {
            ErrorMessageWindow.Visibility = Visibility.Collapsed;

            _openedStream?.Dispose();
            BytesDisplay.Stream = _openedStream = null;

            string hexText = PastedText;
            _openedStream = new MemoryStream();
            int lineNumber = 0;
            using (StringReader reader = new(hexText))
            {
                while (reader.Peek() != 0)
                {
                    lineNumber++;
                    string line = reader.ReadLine();
                    if (line == null) break;
                    if (line.Count(x => x == '|') < 2) break; // not formatted correctly
                    line = line.Substring(line.IndexOf('|') + 1);
                    line = line.Remove(line.IndexOf('|'));
                    line = line.Replace(" ", "").Replace("\t", "").Trim();
                    if (line.Length % 2 != 0) // error data is not div by 2 evenly
                        throw new InvalidDataException($"Line {lineNumber} is not correctly formatted. Must be " +
                            $"even length of Hex bytes (2 characters per byte).");
                    for (int i = 0; i < line.Length; i += 2)
                    {
                        string byteString = line.Substring(i, 2);
                        byte b = byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
                        _openedStream.Write(new byte[] { b });
                    }
                }
            }
            BytesDisplay.Stream = _openedStream;
            ByteLengthLabel.Text = _openedStream.Length.ToString();
        }

        private void HexDisplay_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessageWindow.Visibility = Visibility.Collapsed;
            try
            {
                PasteText(HexDisplay.Text);
            }
            catch(Exception ex)
            {
                ErrorMessageLabel.Text = ex.Message;
                ErrorMessageWindow.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// Shows a window containing the bytes as an image for previewing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewAsImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_openedStream == null) return;
            try
            {
                using var img = System.Drawing.Image.FromStream(_openedStream);
                var imgControl = new Image()
                {
                    Source = img.Convert(),                    
                };
                RenderOptions.SetBitmapScalingMode(imgControl, BitmapScalingMode.NearestNeighbor);
                Window wnd = new()
                {
                    Owner = this,
                    //SizeToContent = SizeToContent.WidthAndHeight,
                    MinWidth = 256,
                    MinHeight = 256,                    
                    Content = imgControl
                };
                // set this here so the window isn't gigantic on open
                //basically pick whatever is smaller the size of window or the height of the image
                wnd.Width = Math.Min(img.Width, this.Width); 
                wnd.Height = Math.Min(img.Height, this.Height);
                wnd.Show();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Open as Image");
                return;
            }
        }
    }
}

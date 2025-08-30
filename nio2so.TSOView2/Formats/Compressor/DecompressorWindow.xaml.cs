using Microsoft.Win32;
using nio2so.Formats.FAR3;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace nio2so.TSOView2.Formats.Compressor
{
    /// <summary>
    /// Interaction logic for DecompressorWindow.xaml
    /// </summary>
    public partial class DecompressorWindow : Window
    {
        enum DIRECTION
        {
            NONE,
            COMPRESSING,
            DECOMPRESSING
        }
        DIRECTION CurrentDirection = DIRECTION.NONE;
        string? InputFileName;

        const string HELPMETEXT = "Quick Tip: \n" +
            "Your RefPack file (if decompressing) MUST start with the RefPack header (0x__FB). " +
            "If this is not the case, then you should use a Hex Editor to make your input file match this format.\n\n" +
            "Note: This tool does use an algorithm to look for this somewhere else in the file, but it is highly recommended to get " +
            "the format to match this requirement for the best results. Decompression will begin after the header has been successfully located.";

        public DecompressorWindow()
        {
            InitializeComponent();

            ResetUI();
        }

        public static void ShowDecompressor() => new DecompressorWindow().Show();

        void ResetUI()
        {
            CompressedDataRect.Visibility = DecompressedDataRect.Visibility = Visibility.Collapsed;
            CompressedLabel.Text = "Compressed File";
            DecompressedLabel.Text = "Decompressed File";
            CompressedSideBoxControl.Background = DecompressedSideBoxControl.Background = Brushes.White;
        }

        private void CompressedSideBoxControl_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (sender == CompressedSideBoxControl)
                CompressedSideBoxControl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB9F1FF"));
            else if (sender == DecompressedSideBoxControl)
                DecompressedSideBoxControl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFE9BA"));
        }

        private void CompressedSideBoxControl_PreviewDragLeave(object sender, DragEventArgs e)
        {
            ResetUI();
        }

        private void DropBoxControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string fName = (e.Data.GetData(DataFormats.FileDrop) as string[])[0]; // get drop action
                DIRECTION direction = DIRECTION.DECOMPRESSING; // set to decompressing by default
                if (sender == DecompressedSideBoxControl)
                    direction = DIRECTION.COMPRESSING; // compressing instead
                try
                {
                    DropInData(fName, direction);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void DropInData(string FileName, DIRECTION CompressAction)
        {
            ResetUI();

            CurrentDirection = CompressAction;
            InputFileName = FileName;

            byte[] inputBytes = File.ReadAllBytes(FileName);
            byte[] outputBytes = null;
            string suggestedFileName = null;
            string filetype_desc = "Compressed RefPack Data|*.dat";

            switch (CompressAction)
            {
                default: return;
                case DIRECTION.COMPRESSING:
                    {
                        //Helpful user sanity check to see if they're submitting an already compressed file                        
                        for(int i = 0; i < Math.Min(inputBytes.Length,20); i++)
                            if (inputBytes[i] == 0xFB)
                            {
                                var result = MessageBox.Show("Please double-check that the file you submitted isn't already compressed.\n\n" +
                                    $"Note: This can be a false-positive of course, I found 0xFB within the first {i} bytes. Continue?",
                                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (result == MessageBoxResult.Yes)
                                    break;
                                else goto exit;
                            }

                        DecompressedLabel.Text = System.IO.Path.GetFileName(FileName);
                        DecompressedDataRect.IsEnabled = false;
                        suggestedFileName = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(FileName),
                            System.IO.Path.GetFileNameWithoutExtension(FileName).Replace("_decompressed", "") + "_compressed.dat");
                        outputBytes = new Decompresser().Compress(inputBytes);
                        CompressedDataRect.Visibility = Visibility.Visible;
                        CompressedLabel.Text = System.IO.Path.GetFileName(suggestedFileName);
                        CompressedDataRect.IsEnabled = true;
                    }
                    break;
                case DIRECTION.DECOMPRESSING:
                    {
                        filetype_desc = "Decompressed RefPack Data|*.dat";
                        //CompressedDataRect.Visibility = Visibility.Visible;
                        CompressedLabel.Text = System.IO.Path.GetFileName(FileName);
                        CompressedDataRect.IsEnabled = false;
                        suggestedFileName = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(FileName),
                            System.IO.Path.GetFileNameWithoutExtension(FileName).Replace("_compressed","") + "_decompressed.dat");
                        outputBytes = new Decompresser().DecompressRefPackStream(inputBytes);
                        DecompressedDataRect.Visibility = Visibility.Visible;
                        DecompressedLabel.Text = System.IO.Path.GetFileName(suggestedFileName);
                        DecompressedDataRect.IsEnabled = true;
                    }
                    break;
            }

            SaveFileDialog sdlg = new SaveFileDialog()
            {
                InitialDirectory = System.IO.Path.GetDirectoryName(suggestedFileName),
                FileName = System.IO.Path.GetFileName(suggestedFileName),
                Filter = filetype_desc
            };
            if (!(!sdlg.ShowDialog() ?? true))            
                File.WriteAllBytes(sdlg.FileName, outputBytes);    
        exit:
            ResetUI();
        }

        private void DecompressedDataRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void HelpMeButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show(HELPMETEXT);        
    }
}

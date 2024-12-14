using Microsoft.Win32;
using nio2so.Formats.FAR3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            "Your RefPack file (if decompressing) MUST start with uint32 Size then 0x10FB as the RefPack magic " +
            "number. If this is not the case, then you must use a Hex Editor to make your input file match this requirement.\n\n" +
            "This tool does use a simple algorithm to try to find this somewhere in the file, but it is highly recommended to get " +
            "the format to match this requirement for the best results.";

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

            switch (CompressAction)
            {
                default: return;
                case DIRECTION.COMPRESSING:
                    {
                        //DecompressedDataRect.Visibility = Visibility.Visible;
                        DecompressedLabel.Text = System.IO.Path.GetFileName(FileName);
                        DecompressedDataRect.IsEnabled = false;
                        suggestedFileName = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(FileName),
                            System.IO.Path.GetFileNameWithoutExtension(FileName) + "_compressed.dat");
                        outputBytes = new Decompresser().Compress(inputBytes);
                        CompressedDataRect.Visibility = Visibility.Visible;
                        CompressedLabel.Text = System.IO.Path.GetFileName(suggestedFileName);
                        CompressedDataRect.IsEnabled = true;
                    }
                    break;
                case DIRECTION.DECOMPRESSING:
                    {
                        //CompressedDataRect.Visibility = Visibility.Visible;
                        CompressedLabel.Text = System.IO.Path.GetFileName(FileName);
                        CompressedDataRect.IsEnabled = false;
                        suggestedFileName = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(FileName),
                            System.IO.Path.GetFileNameWithoutExtension(FileName) + "_decompressed.dat");
                        outputBytes = new Decompresser().Decompress(inputBytes);
                        DecompressedDataRect.Visibility = Visibility.Visible;
                        DecompressedLabel.Text = System.IO.Path.GetFileName(suggestedFileName);
                        DecompressedDataRect.IsEnabled = true;
                    }
                    break;
            }

            SaveFileDialog sdlg = new SaveFileDialog()
            {
                InitialDirectory = System.IO.Path.GetDirectoryName(suggestedFileName),
                FileName = System.IO.Path.GetFileName(suggestedFileName)
            };
            if (!(!sdlg.ShowDialog() ?? true))            
                File.WriteAllBytes(sdlg.FileName, outputBytes);                                       
            ResetUI();
        }

        private void DecompressedDataRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void HelpMeButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show(HELPMETEXT);        
    }
}

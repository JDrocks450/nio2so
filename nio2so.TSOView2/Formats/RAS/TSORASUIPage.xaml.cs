using Microsoft.Win32;
using nio2so.Formats.Streams;
using nio2so.Voltron.Core.TSO.Serialization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.RAS
{
    /// <summary>
    /// Interaction logic for TSORASUIPage.xaml
    /// </summary>
    public partial class TSORASUIPage : Page
    {
        public RASStream? Stream { get; private set; }

        public TSORASUIPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Stream == null)
                PromptAndOpenArchive();
        }

        public void UpdateUI()
        {
            //**clear state
            ChunksListBox.SelectionChanged -= ChunksListBox_SelectionChanged; 
            ChunksListBox.ItemsSource = TableOfContentsListBox.ItemsSource = null;
            HexEditor.Stream?.Dispose();
            HexEditor.Stream = null;
            HeaderDataGrid.ItemsSource = null;

            //** load state
            if (Stream == null) return;

            ChunksListBox.ItemsSource = Stream.Content.Chunks.Values;
            TableOfContentsListBox.ItemsSource = Stream.Content.TableOfContents.Values;

            ChunksListBox.SelectionChanged += ChunksListBox_SelectionChanged;
            TableOfContentsListBox.SelectionChanged += ChunksListBox_SelectionChanged;

            HeaderDataGrid.ItemsSource = Stream.Header.GetDescriptionStrings();
        }

        private void ChunksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var value = (sender as ListBox).SelectedValue;
            uint ChunkType = 0;
            if (value is RASStream.RASTableOfContents.RASTOCEntry tocEntry)
            {
                ChunkType = tocEntry.ChunkType;
                ChunksListBox.SelectedItem = Stream.Content.GetChunk(ChunkType);
                return;
            }
            if (value is RASStream.RASArchive.RASChunk chunkData)
            {
                MemoryStream stream = new(chunkData.Content);
                HexEditor.Stream = stream;
                return;
            }
        }

        public void OpenArchive(FileInfo FileDesc)
        {
            byte[] fileData = File.ReadAllBytes(FileDesc.FullName);
            Stream = TSOVoltronSerializer.Deserialize<RASStream>(fileData);

            byte[] reserializedData = TSOVoltronSerializer.Serialize(Stream);
            File.WriteAllBytes(@"C:\Users\Jeremy\OneDrive\Desktop\" + FileDesc.Name + ".bin", reserializedData); // for debugging

            if (!RASStream.VerifyIntegrity(fileData, reserializedData))
                MessageBox.Show($"Warning: The RAS Stream at {FileDesc.FullName} failed integrity verification! This may indicate that the file is malformed or that there is an issue with the deserialization/serialization code.", $"Integrity Verification Failed", MessageBoxButton.OK, MessageBoxImage.Warning);

            UpdateUI();
        }

        public bool PromptAndOpenArchive(string InitialDirectory = default)
        {
            string Prompt = $"Open a \"RAS_\" Stream";

            if (InitialDirectory == null)
            {
                if (TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory != null)
                    InitialDirectory = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;
                else InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }
            OpenFileDialog fileDialog = new()
            {
                Title = Prompt,
                InitialDirectory = InitialDirectory,
                Multiselect = false,
                DereferenceLinks = true,
                ValidateNames = true,
            };
            if (fileDialog.ShowDialog() == true)
            {
                try
                { // default error handler
                    OpenArchive(new FileInfo(fileDialog.FileName));
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open RAS Stream:\n{ex.Message}", $"Error Opening a \"RAS_\" Stream", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }
    }
}

using Microsoft.Win32;
using nio2so.Formats.ARCHIVE;
using nio2so.Formats.FAR1;
using nio2so.Formats.FAR3;
using nio2so.Formats.Img.Targa;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace nio2so.TSOView2.Formats.FAR3
{
    /// <summary>
    /// Interaction logic for FAR3Control.xaml
    /// </summary>
    public partial class FAR3Control : Page, IDisposable
    {
        public enum FARMode
        {
            FAR1= 0,
            FAR1_v1B = 1,
            FAR3 = 2,
        }

        string? archivePath;
        IFileArchive<string>? archive;
        private readonly FARMode mode;

        public bool ArchiveOpened => archive != null;

        public FAR3Control(FARMode Mode = FARMode.FAR3)
        {
            InitializeComponent();

            Unloaded += delegate { Dispose(); };
            Loaded += FAR3Control_Loaded;
            UIReset();
            mode = Mode;
        }
        ~FAR3Control()
        {
            Dispose();
        }

        private void FAR3Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ArchiveOpened)
                PromptAndOpenArchive();
        }

        void UIReset()
        {
            ExtractDirectoryLabel.Visibility = Visibility.Collapsed;
            ImagePreviewBox.Source = null;
            FileTree.Items.Clear();
        }

        void UIRedraw()
        {
            FileTree.Items.Clear();

            if (!ArchiveOpened || archive == null) return;
            lock (archive)
            {
                //make file root tree node
                TreeViewItem root = new TreeViewItem()
                {
                    Header = System.IO.Path.GetFileName(archivePath),
                    IsExpanded = true
                };
                root.ItemsSource = archive.GetAllFileEntries();
                FileTree.Items.Add(root);
            }
        }

        void CopyImage()
        {
            BitmapSource? src = ImagePreviewBox?.Source as BitmapSource;
            if (src == null) return;
            Clipboard.SetImage(src);
        }

        void UIUpdatePreview(IFileEntry Entry)
        {
            if (!ArchiveOpened || archive == null) return;
            if (Entry == null) return;
            if (string.IsNullOrWhiteSpace(Entry.Filename)) return;
            byte[] entryData = default;
            lock (archive)
            {
                entryData = archive.GetEntry(Entry.Filename);                
            }
            if (entryData == null) return;
            using MemoryStream stream = new MemoryStream(entryData);

            try
            {
                using System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                ImagePreviewBox.Source = img.Convert(true);
                return;
            }
            catch { } // not supported?
            stream.Seek(0,SeekOrigin.Begin);
            try
            {
                using var tga = TargaImage.LoadTargaImage(stream);
                ImagePreviewBox.Source = tga.Convert(true);
            }
            catch { }
        }

        async Task<(bool Result, string ErrorReason)> ExtractOne(string BaseDirectory, IFileEntry Entry)
        {
            IFileEntry entry = Entry;
            string ErrorReason = "An entry was null. Skipping this entry.";
            if (entry == null)
                return (false, ErrorReason);
            ErrorReason = "An archive is not open right now.";
            if (!ArchiveOpened)
                return (false, ErrorReason);
            string name = entry.ToString();
            bool success = false;
            try
            {
                _ = Dispatcher.InvokeAsync(() =>
                {
                    ExtractDirectoryLabel.Visibility = Visibility.Visible;
                    (ExtractionDirectoryLabel.Inlines.ElementAt(0) as Run).Text = BaseDirectory;                    
                });
                await File.WriteAllBytesAsync(System.IO.Path.Combine(BaseDirectory, name), archive[name]);
                ErrorReason = "OK.";
                success = true;
            }
            catch (UnauthorizedAccessException)
            {// challenge user to change output dir
                ErrorReason = "TSOVIEW_PROMPT_NEWDIR";
            }
            catch (Exception ex)
            {
                ErrorReason = ($"{name} was not extracted. {ex.Message}");
            }
            return (success, ErrorReason);
        }

        public void OpenArchive(FileInfo FileName, FARMode? Mode = default)
        {
            if (!Mode.HasValue)
                Mode = mode;
            if (!FileName.Exists)
                throw new FileNotFoundException(FileName.FullName);
            archivePath = FileName.FullName;         
            switch (Mode)
            {
                case FARMode.FAR3:
                    archive = new FAR3Archive(FileName.FullName);
                    break;
                case FARMode.FAR1:
                    archive = new FAR1Archive(FileName.FullName, false);
                    break;
                case FARMode.FAR1_v1B:
                    archive = new FAR1Archive(FileName.FullName, true);
                    break;
            }            
            UIRedraw();
        }

        public bool PromptAndOpenArchive(string InitialDirectory = default)
        {
            string Prompt = $"Open a {mode} Archive";

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
                    MessageBox.Show($"Failed to open {mode} Archive:\n{ex.Message}", $"Error Opening {mode} Archive", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        async Task ExtractAll(bool ForceDirBrowser = false, params IFileEntry[] Entries)
        {
            if (!ArchiveOpened) return;
            StringBuilder errorBuilder = new();
            string? baseDir = System.IO.Path.GetDirectoryName(archivePath);
            if (ForceDirBrowser) baseDir = default;
            if (baseDir == default) PromptNewDir();
            bool PromptNewDir()
            {
                OpenFolderDialog fld = new OpenFolderDialog()
                {
                    Title = "Pick Extraction Directory",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Multiselect = false,
                    ValidateNames = true,
                    DereferenceLinks= true,                    
                };
                if (!fld?.ShowDialog() ?? true) return false;
                baseDir = fld.FolderName;
                return true;
            }
            bool Nested = false;
        retry:
            foreach (var entry in Entries)
            {
                var valueResult = await ExtractOne(baseDir, entry);
                if (!valueResult.Result)
                {
                    if (valueResult.ErrorReason == "TSOVIEW_PROMPT_NEWDIR")
                    {
                        if (!Nested)
                        {// select new dir
                            Nested = true;
                            PromptNewDir();
                            goto retry;
                        }
                        valueResult.ErrorReason = "Cannot access the directory: " + baseDir;
                    }
                    errorBuilder.AppendLine(valueResult.ErrorReason);
                }
            }
            if (errorBuilder.Length > 0)
                MessageBox.Show(errorBuilder.ToString(), "Completed with Errors");
        }

        private async void AllButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ArchiveOpened || archive == null) return;
            bool promptDir = false;
            if (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down) promptDir = true;
            await ExtractAll(promptDir, archive.GetAllFileEntries().ToArray());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        { // REFRESH
            UIRedraw();
        }

        internal static void SpawnWindow(FARMode Mode) => new Window() { Content = new FAR3Control(Mode) }.Show();

        private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!ArchiveOpened) return;
            if (FileTree.SelectedItem == null) return;
            if (FileTree.SelectedItem is IFileEntry entry)
                UIUpdatePreview(entry);
        }

        public void Dispose()
        {
            UIReset();
            archivePath = null;
            archive?.Dispose();
            archive = null;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e) => CopyImage();

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.C)) 
                CopyImage();
        }

        private async void FileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ArchiveOpened) return;
            if (FileTree.SelectedItem == null) return;
            await ExtractAll(false, (IFileEntry)FileTree.SelectedItem);
        }

        private void ExtractionDirectoryLabel_Click(object sender, RoutedEventArgs e)
        {
            string fLink = (ExtractionDirectoryLabel.Inlines.ElementAt(0) as Run).Text;
            using Process p = Process.Start("explorer", fLink);
        }
    }
}

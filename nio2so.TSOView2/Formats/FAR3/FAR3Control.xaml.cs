using Microsoft.Win32;
using nio2so.Formats.ARCHIVE;
using nio2so.Formats.FAR1;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.FAR3
{
    /// <summary>
    /// Interaction logic for FAR3Control.xaml
    /// </summary>
    public partial class FAR3Control : Page
    {
        public enum FARMode
        {
            FAR1= 0,
            FAR1_v1B = 1,
            FAR3 = 2,
        }

        string archivePath;
        IFileArchive<string>? archive;
        private readonly FARMode mode;

        public bool ArchiveOpened => archive != null;

        public FAR3Control(FARMode Mode = FARMode.FAR3)
        {
            InitializeComponent();

            Loaded += FAR3Control_Loaded;
            mode = Mode;
        }

        private void FAR3Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ArchiveOpened)
                PromptAndOpenArchive();
        }

        void UIRedraw()
        {
            FileTree.Items.Clear();

            if (!ArchiveOpened) return;
            //make file root tree node
            TreeViewItem root = new TreeViewItem()
            {
                Header = System.IO.Path.GetFileName(archivePath),
                IsExpanded = true
            };
            root.ItemsSource = archive.GetAllFileEntries();
            FileTree.Items.Add(root);
        }

        async Task<(bool Result, string ErrorReason)> ExtractOne(string BaseDirectory, IFileEntry Entry)
        {                        
            IFileEntry? entry = Entry;
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

        async Task ExtractAll(bool ForceDirBrowser = false)
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
            foreach (var entry in archive.GetAllFileEntries())
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
            bool promptDir = false;
            if (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down) promptDir = true;
            await ExtractAll(promptDir);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        { // REFRESH
            UIRedraw();
        }

        internal static void SpawnWindow(FARMode Mode) => new Window() { Content = new FAR3Control(Mode) }.Show();
    }
}

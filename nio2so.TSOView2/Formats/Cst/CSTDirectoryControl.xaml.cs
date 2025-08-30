using Microsoft.Win32;
using nio2so.Formats.CST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace nio2so.TSOView2.Formats.Cst
{
    /// <summary>
    /// Interaction logic for CSTDirectoryControl.xaml
    /// </summary>
    public partial class CSTDirectoryControl : Page
    {
        public CSTDirectory? Directory { get; private set; }
        public bool IsDirectoryOpen => Directory != null;

        private Task<CSTFile>? loadingTask;

        public CSTDirectoryControl()
        {
            InitializeComponent();
        }

        void ReevaluateUIComponents()
        {
            DirectoryTree.Items.Clear();
            TreeViewItem rootItem = new ()
            {
                Header = IsDirectoryOpen ? $"CST Directory - {Directory!.Count} files loaded" : "No CST Directory Loaded",
                IsExpanded = true,
            };
            DirectoryTree.Items.Add(rootItem);

            if (IsDirectoryOpen && Directory != null)
            {
                for (int i = 0; i < Directory.Count; i++)
                {
                    //get file info
                    uint fileKey = Directory.Keys.ElementAt(i);
                    CSTFile file = Directory[fileKey];
                    string fName = System.IO.Path.GetFileName(file.FilePath) ?? "unknown file";
                    string displayName = $"{fileKey:D4}: {fName}";
                    //make a new tree view item for this file
                    TreeViewItem newItem = new ()
                    {
                        Header = displayName,
                        Tag = file,
                    };
                    newItem.Selected += CSTFileSelected;
                    rootItem.Items.Add(newItem);
                }
            }
        }

        private async void CSTFileSelected(object sender, RoutedEventArgs e)
        {
            if ((sender is TreeViewItem item) && (item.Tag is CSTFile file))
            {
                DirectoryTree.IsEnabled = false;
                try
                { // this can sometimes take a while
                    file = await LazyLoadCSTFile(file);
                }
                catch
                {
                    goto error;
                }
                finally
                {
                    DirectoryTree.IsEnabled = true;
                }
                //utilize anonymous type to display the file contents in the grid without a definite type
                CSTDisplayGrid.ItemsSource = file.Select(x => new
                {
                    EntryName = x.Key,
                    Text = x.Value,
                    Comment = x.Value.Comment
                });
                return;
            }
        error:
            DirectoryTree.IsEnabled = true;
            //make a generic error object
            CSTDisplayGrid.ItemsSource = new[] { 
                new
                {
                    ErrorValue = "Selected item is not a valid CST File or has no value set. Perhaps the file cannot be read?"
                }
            };
        }

        Task<CSTFile> LazyLoadCSTFile(CSTFile file) => Task.Run(() => CSTImporter.Import(file.FilePath));

        /// <summary>
        /// Opens a CST Directory from the provided path and populates the control with its contents.
        /// <para/>Also populates the internal <see cref="Directory"/> property.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void OpenDirectory(DirectoryInfo path)
        {
            CloseDirectory();
            if (!path.Exists)
                throw new DirectoryNotFoundException($"The directory '{path.FullName}' does not exist.");
            Directory = CSTImporter.ImportDirectory(path.FullName);

            Dispatcher.Invoke(ReevaluateUIComponents, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        /// <summary>
        /// Prompts the user to select a CST Directory to open -- then uses <see cref="OpenDirectory(DirectoryInfo)"/> to open it.
        /// <para/>This function will alert the user if an error occurs while opening the directory, exceptions are handled for OpenDirectory only.
        /// </summary>
        /// <param name="Prompt"></param>
        /// <param name="InitialDirectory"></param>
        public bool PromptAndOpenDirectory(string Prompt = "Open a UIText.dir Directory with *.CST files in it", string? InitialDirectory = default)
        {
            //Smart defaulting behavior for directory selection
            if (InitialDirectory == null || !System.IO.Directory.Exists(InitialDirectory))
            {
                if (TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory == null)
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);// default to Program Files if we have no clue
                else // default to TSO GameData/UIText.dir as in both games is where the CST directory is located
                {
                    InitialDirectory = System.IO.Path.Combine(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory, "GameData");
                    if (System.IO.Directory.Exists(System.IO.Path.Combine(InitialDirectory, "UIText.dir"))) // safely step into UIText.dir if it exists
                    {
                        InitialDirectory = System.IO.Path.Combine(InitialDirectory, "UIText.dir"); // safely step in english.dir if it exists (release tso)
                        if (System.IO.Directory.Exists(System.IO.Path.Combine(InitialDirectory, "english.dir")))
                            InitialDirectory = System.IO.Path.Combine(InitialDirectory, "english.dir");
                    }
                }
            }
            OpenFolderDialog fileDialog = new()
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
                    OpenDirectory(new DirectoryInfo(fileDialog.FolderName));
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open CST Directory:\n{ex.Message}", "Error Opening CST Directory", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }
        /// <summary>
        /// Closes the currently opened CST Directory, if any.
        /// </summary>
        public void CloseDirectory()
        {
            if (!IsDirectoryOpen) return;
            Directory = null;
        }
    }
}

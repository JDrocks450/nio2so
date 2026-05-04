using Microsoft.Win32;
using nio2so.Formats.CST;
using nio2so.Formats.UI.TSOTheme;
using nio2so.Formats.UI.UIScript;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace nio2so.TSOView2.Formats.UIs
{
    internal class UIsHandler
    {
        internal static UIsHandler Current { get; private set; }

        /// <summary>
        /// The <see cref="UIScriptFile"/> the User opened
        /// </summary>
        public UIScriptFile CurrentFile { get; private set; }
        /// <summary>
        /// The file name the User supplied to visit <see cref="CurrentFile"/>
        /// </summary>
        public string CurrentFilePath { get; private set; }
        /// <summary>
        /// The current theme -- the map of AssetIDs to images stored in the game directory
        /// </summary>
        public TSOThemeFile CurrentTheme { get; private set; }
        /// <summary>
        /// The imported UIText directory
        /// </summary>
        public CSTDirectory StringTables { get; private set; }

        private TSOUIScriptImporter defaultImporter = new();

        public UIsHandler()
        {
            Current = this;
        }

        public void Initialize()
        {
            try
            {
                TSOViewConfigHandler.LoadFromFile();
            }
            catch (Exception e)
            {
                File.Delete(TSOViewConfigHandler.PATH);
                MessageBox.Show($"Your configuration settings are corrupted. {e.Message}\n\n" +
                    $"Resetting to default settings.", $"{nameof(TSOViewConfig)} File");
            }
            finally
            {
                if (TSOViewConfigHandler.CurrentConfiguration == default)
                    TSOViewConfigHandler.CurrentConfiguration = new();
            }
            if (!TSOViewConfigHandler.EnsureSetGameDirectoryFirstRun()) return;
            //game directory not set!!

            try
            { // LOAD THEME FILE
                var file = TSOThemeFileImporter.Import(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnlinePreAlpha_ThemePath);
                if (file != null)
                    CurrentTheme = file;
            }
            catch (FileNotFoundException e)
            {

            }
            if (CurrentTheme == null)
                CurrentTheme = new TSOThemeFile(TSOThemeFile.ThemeVersionNames.NotSet);
            ChangeGameDirectory();
        }

        public void ChangeGameDirectory(string NewGameDirectoryPath = default)
        {
            //WHEN GAME DIRECTORY CHANGES MID-SESSION, RUN THESE
            ;

            if (TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory != NewGameDirectoryPath)
            {
                //ONLY RUN THESE WHEN THE PATH CHANGES

                // (RE)LOAD CST FILE
                try
                {
                    var directory = CSTImporter.ImportDirectory(
                        System.IO.Path.Combine(
                            TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory, "UIText.dir"
                        ), out string[] Errors);
                    if (directory != null)
                        StringTables = directory;
                    defaultImporter.SetCST(StringTables);
                    if (Errors != null && Errors.Length > 0)
                        MessageBox.Show("The following CST files were not imported:\n\n" +
                            $"{string.Join('\n', Errors)}");
                }
                catch (DirectoryNotFoundException e)
                {
                    MessageBox.Show($"You may want to double-check the chosen directory: " +
                        $"{NewGameDirectoryPath ?? TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory ?? "Unknown"}." +
                        $"\n{e.Message}\nPlease restart the application.");
                    TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory = null;
                    TSOViewConfigHandler.SaveConfiguration();
                    Application.Current.Shutdown();
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"There was a problem loading CSTs (Strings) from your The Sims Online installation. \n" +
                        $"{e.Message}. The UIScript Editor may not be usable.");
                }

                if (NewGameDirectoryPath != default)
                { // ONLY RUN THESE WHEN THE PATH CHANGES AND IS NOT NULL
                    TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory = NewGameDirectoryPath;
                    TSOViewConfigHandler.SaveConfiguration();
                }
            }
        }

        /// <summary>
        /// Closes the current session and frees all resources related to it.
        /// </summary>
        private void FinishUp()
        {
            CurrentFile = null;
        }

        /// <summary>
        /// Closes the current session if applicable and prompts the user to open a new *.uis file.
        /// </summary>
        public void PromptUserOpenFile()
        {
            //CLOSE PREV SESSION
            FinishUp();
            //Open a new file
            OpenFileDialog dlg = new()
            {
                AddExtension = true,
                DefaultExt = "*.uis",
                CheckFileExists = true,
                RestoreDirectory = true,
                InitialDirectory = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_UIScriptsDirectory,
                Filter = "The Sims Online UI Script|*.uis",
                Multiselect = false,
                Title = "Open a UIScript File..."
            };
            if (!dlg.ShowDialog() ?? true)
                return; // user dismissed the dialog away
            try
            {
                CurrentFile = defaultImporter.ImportFromFile(dlg.FileName);
                CurrentFilePath = dlg.FileName;
                OnOK();
            }
            catch(Exception e)
            {
                MessageBox.Show($"An error occured while parsing that file: \n{e.Message}");
            }
            finally
            {

            }
            void OnOK()
            {
                TSOUIDialogViewerPage page = new();
                (Application.Current.MainWindow as ITSOView2Window).ShowPlugin(page);
            }
        }
    }
}

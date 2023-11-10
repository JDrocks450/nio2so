using Microsoft.Win32;
using nio2so.Formats.CST;
using nio2so.Formats.UI.TSOTheme;
using nio2so.Formats.UI.UIScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace nio2so.TSOView2.Formats.UIs
{
    internal class UIsHandler
    {
        // HANDLER STATIC
        internal static UIsHandler Current { get; private set; }
        static UIsHandler()
        {
            Current = new UIsHandler();
        }
        //----

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
        public TSOThemeFile CurrentTheme { get; private set; } = new();
        /// <summary>
        /// The imported UIText directory
        /// </summary>
        public CSTDirectory StringTables { get; private set; }

        private TSOUIScriptImporter defaultImporter = new();

        private UIsHandler()
        {
            try
            { // LOAD THEME FILE
                var file = TSOThemeFileImporter.Import(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnlinePreAlpha_ThemePath);
                if (file != null)
                    CurrentTheme = file;
            }
            catch { }
            // LOAD CST FILE
            var directory = CSTImporter.ImportDirectory(
                System.IO.Path.Combine(
                    TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory, "UIText.dir"
                ));
            if (directory != null)
                StringTables = directory;
            defaultImporter.SetCST(StringTables);
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
                (Application.Current.MainWindow as ITSOView2Window).MainWindow_ShowPlugin(page);
            }
        }
    }
}

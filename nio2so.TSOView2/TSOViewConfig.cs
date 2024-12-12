using Microsoft.Win32;
using nio2so.Formats.CST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nio2so.TSOView2
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class TSOViewConfigItem : Attribute
    {
        
    }

    [Serializable]
    public class TSOViewConfig
    {
        public TSOViewConfig()
        {
        }
        public TSOViewConfig(string? BaseDirectory) : this() => TheSimsOnline_BaseDirectory = BaseDirectory;
                
        public string TheSimsOnlinePreAlpha_ThemePath => Path.Combine(Environment.CurrentDirectory, "Themes", "tsopa.tsotheme");

        /// <summary>
        /// The base directory of The Sims Online: Pre-Alpha
        /// </summary>
        [JsonInclude]
        [TSOViewConfigItem]
        public string? TheSimsOnline_BaseDirectory { get; set; } = default;
        /// <summary>
        /// The base directory of the Sims Online: New and Improved
        /// </summary>
        [JsonInclude]
        [TSOViewConfigItem]
        public string? TheSimsOnline_NI_BaseDirectory { get; set; } = default;
        [JsonIgnore]        
        public string? TheSimsOnline_GameDataDirectory => 
            TheSimsOnline_BaseDirectory != default ? Path.Combine(TheSimsOnline_BaseDirectory, "GameData") : default;
        [JsonIgnore]
        public string? TheSimsOnline_UIGraphicsDirectory =>
            TheSimsOnline_BaseDirectory != default ? Path.Combine(TheSimsOnline_BaseDirectory, "UIGraphics") : default;
        [JsonIgnore]
        public string? TheSimsOnline_UIScriptsDirectory =>
            TheSimsOnline_GameDataDirectory != default ? Path.Combine(TheSimsOnline_GameDataDirectory, "UIScripts") : default;


        public string? GetDirectoryByVersion(TSOVersion version) => version == TSOVersion.PreAlpha ? TheSimsOnline_BaseDirectory : TheSimsOnline_BaseDirectory;
    }

    public enum TSOVersion
    {
        PreAlpha,
        NewImproved
    }

    internal static class TSOViewConfigHandler
    {
        

        private const string PATH = "tsoview2.config";

        static TSOViewConfigHandler()
        {
            CurrentConfiguration = new()
            {
                TheSimsOnline_BaseDirectory = null
            };
        }

        public static TSOViewConfig CurrentConfiguration { get; set; }

        /// <summary>
        /// Spawns a dialog that displays the current <see cref="TSOViewConfig"/> properties
        /// </summary>
        public static void InvokeConfigViewerDialog()
        {
            Window wnd = new()
            {
                Title = "TSOView2 Configuration Settings",
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new DataGrid()
                {
                    ItemsSource = new[] { CurrentConfiguration },
                    Margin = new Thickness(10)
                },
                Width = 600,
                Height = 350
            };
            wnd.Closed += delegate
            {
                SaveConfiguration();
            };
            wnd.Show();
        }

        /// <summary>
        /// Ensures the Game Directory is set, if not, will prompt the user to do so now. 
        /// True if Game Directory has a value at the end of the function's lifetime
        /// </summary>
        public static bool EnsureSetGameDirectoryFirstRun()
        {
            string? basePath = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;
            //NO CHOSEN THE SIMS ONLINE DIRECTORY ERROR
            if (basePath == null)
            {
                if (MessageBox.Show("You haven't selected a The Sims Online directory yet. Would you like to do so now?",
                    "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return false;
                TSOViewConfigHandler.Directory_PromptAndSaveResult("Select any file in your The Sims Online Directory",
                    ref basePath);
                if (basePath == null) return false;
                TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory = basePath;
                TSOViewConfigHandler.SaveConfiguration();
            }
            return TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory != null;
        }        

        /// <summary>
        /// Prompts the user to select a file and returns the directory it's in.
        /// </summary>
        /// <returns></returns>
        public static bool Directory_PromptAndSaveResult(string Prompt, ref string DirectoryResult)
        {
            OpenFileDialog fileDialog = new()
            {
                Title = Prompt,
                InitialDirectory = DirectoryResult,
                Multiselect = false,
                DereferenceLinks = true,
                CheckFileExists = true,
            };
            if (fileDialog.ShowDialog() ?? false)
            {
                DirectoryResult = Path.GetDirectoryName(fileDialog.FileName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads from the default location to the <see cref="CurrentConfiguration"/> property
        /// </summary>
        /// <returns></returns>
        public static bool LoadFromFile()
        {
            if (!File.Exists(PATH))
                return false;
            CurrentConfiguration = JsonSerializer.Deserialize<TSOViewConfig>(File.ReadAllText(PATH));
            return true;
        }

        internal static void SaveConfiguration() => File.WriteAllText(PATH, JsonSerializer.Serialize<TSOViewConfig>(CurrentConfiguration));
    }
}

using Microsoft.Win32;
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
    [Serializable]
    public class TSOViewConfig
    {
        public TSOViewConfig()
        {
        }
        public TSOViewConfig(string? BaseDirectory) : this() => TheSimsOnline_BaseDirectory = BaseDirectory;

        public string TheSimsOnlinePreAlpha_ThemePath => Path.Combine(Environment.CurrentDirectory, "Themes", "tsopa.tsotheme");

        [JsonInclude]
        public string? TheSimsOnline_BaseDirectory { get; set; } = default;
        [JsonIgnore]
        public string? TheSimsOnline_GameDataDirectory => 
            TheSimsOnline_BaseDirectory != default ? Path.Combine(TheSimsOnline_BaseDirectory, "GameData") : default;
        [JsonIgnore]
        public string? TheSimsOnline_UIGraphicsDirectory =>
            TheSimsOnline_BaseDirectory != default ? Path.Combine(TheSimsOnline_BaseDirectory, "UIGraphics") : default;
        [JsonIgnore]
        public string? TheSimsOnline_UIScriptsDirectory =>
            TheSimsOnline_GameDataDirectory != default ? Path.Combine(TheSimsOnline_GameDataDirectory, "UIScripts") : default;
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
            wnd.Show();
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

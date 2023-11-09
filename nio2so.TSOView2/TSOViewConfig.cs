using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        static TSOViewConfigHandler()
        {
            CurrentConfiguration = new()
            {
                TheSimsOnline_BaseDirectory = @"E:\Games\TSO Pre-Alpha\TSO"
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
    }
}

using Microsoft.Win32;
using nio2so.Formats.Terrain;
using nio2so.TSOView2.Formats.UIs;
using nio2so.TSOView2.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nio2so.TSOView2.Formats.Terrain
{
    internal class CityTerrainHandler
    {
        public static CityTerrainHandler Current { get; private set; }
        public TSOCity City { get; }
        public TSOCityMesh Mesh { get; }
        public TSOCityContentManager ContentManager => TSOCityImporter.GlobalDefaultContent;

        private CityTerrainHandler(TSOCity city, TSOCityMesh mesh)
        {
            Current = this;
            City = city;
            Mesh = mesh;
        }

        public static async Task<CityTerrainHandler?> PromptLoadCity(TSOVersion Version)
        {
            //IS THE DIRECTORY SELECTED? 
            if (!TSOViewConfigHandler.EnsureSetGameDirectoryFirstRun()) return null; // idk anymore

            //TSO CONFIG FILE
            string? gamePath = TSOViewConfigHandler.CurrentConfiguration.GetDirectoryByVersion(Version);
            if (gamePath == default) throw new NullReferenceException("Game directory not set.");

            //PROMPT FOR CITY DIRECTORY
            OpenFileDialog dlg = new()
            {
                AddExtension = true,
                DefaultExt = "*.uis",
                CheckFileExists = true,
                RestoreDirectory = true,
                InitialDirectory = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory +"\\FarZoom",
                Filter = "The Sims Online City Bitmap|*.bmp",
                Multiselect = false,
                Title = "Open any City file in a City folder..."
            };
            if (!dlg.ShowDialog() ?? true)
                return default; // user dismissed the dialog away
                                // 
            return await LoadCity(Path.GetDirectoryName(dlg.FileName), Version);
        }        

        public static async Task<CityTerrainHandler?> LoadCity(string FolderName, TSOVersion Version)
        {
            //TSO CONFIG FILE
            string? gamePath = TSOViewConfigHandler.CurrentConfiguration.GetDirectoryByVersion(Version);
            if (gamePath == default) throw new NullReferenceException("Game directory is not selected.");

            string verb = "parsing";
            try
            {
                //CREATE CITY LOADER
                TSOPreAlphaCityImporter importer = new(gamePath, FolderName)
                {
                    CitySettings = new()
                    {
                        ElevationScale = 1 / 8.0
                    }
                };
                //LOAD ASSETS
                verb = "loading assets for";
                await importer.LoadAssetsAsync(Version == TSOVersion.PreAlpha); // asset loader
                //LOAD GEOMETRY TO MEM
                verb = "generating geometry for";
                (TSOCity city, TSOCityMesh mesh) values = await importer.LoadCityAsync(); // parse assets into city render                                                                                          
                values.mesh.FixNormals(values.city); // mandatory                                  

                //LOAD PLUG-IN
                return new CityTerrainHandler(values.city, values.mesh);
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occured while {verb} that city: \n{e.Message}");
            }
            return default; // default fail-out
        }

        public static async void PromptUserShowCityPlugin() => await PromptUserShowCityPlugin(TSOVersion.PreAlpha);

        public static async Task PromptUserShowCityPlugin(TSOVersion Version)
        {
            var handler = await PromptLoadCity(Version);
            if (handler == null) return; // process above failure

            TSOCityViewPage page = new();
            page.Mini_HUD_Image.Source = Current.Mesh.VertexColorAtlas.Convert();

            (Application.Current.MainWindow as ITSOView2Window).MainWindow_ShowPlugin(page);
        }
    }
}

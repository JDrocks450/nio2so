using nio2so.Formats.Terrain;
using nio2so.TSOView2.Util;
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

        public static async Task<CityTerrainHandler> LoadCity()
        {
            //TSO CONFIG FILE
            string gamePath = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;

            //CITY LOADER
            TSOPreAlphaCityImporter importer = new(gamePath, "D:\\Games\\FreeSO\\The Sims Online\\TSOClient\\cities\\city_0002")
            {
                CitySettings = new()
                {
                    ElevationScale = 1/15.0
                }
            };
            await importer.LoadAssetsAsync(false); // asset loader
            (TSOCity city, TSOCityMesh mesh) values = await importer.LoadCityAsync(); // parse assets into city render            
            values.mesh.FixNormals(values.city); // mandatory

            
            Window debugWindow = new Window()
            {
                Content = new Image()
                {
                    Source = importer.Debug_ElevationBmp.Convert()
                }
            };
            debugWindow.Show();
                      

            return new CityTerrainHandler(values.city, values.mesh);
        }

        public void ShowCityPlugin()
        {
            TSOCityViewPage page = new();
            (Application.Current.MainWindow as ITSOView2Window).MainWindow_ShowPlugin(page);
        }
    }
}

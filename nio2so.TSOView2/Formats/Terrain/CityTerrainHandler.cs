using nio2so.Formats.Terrain;
using System.Threading.Tasks;
using System.Windows;

namespace nio2so.TSOView2.Formats.Terrain
{
    internal class CityTerrainHandler
    {
        public static CityTerrainHandler Current { get; private set; }
        public TSOCity City { get; }
        public TSOCityMesh Mesh { get; }

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
            TSOPreAlphaCityImporter importer = new(gamePath)
            {
                CitySettings = new()
                {
                    ElevationScale = 1/15.0
                }
            };
            await importer.LoadAssetsAsync(); // asset loader
            (TSOCity city, TSOCityMesh mesh) values = await importer.LoadCityAsync(); // parse assets into city render            
            values.mesh.FixNormals(values.city); // mandatory

            return new CityTerrainHandler(values.city, values.mesh);
        }

        public void ShowCityPlugin()
        {
            TSOCityViewPage page = new();
            (Application.Current.MainWindow as ITSOView2Window).MainWindow_ShowPlugin(page);
        }
    }
}

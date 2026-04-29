using nio2so.Formats.Terrain;
using nio2so.TSOView2.Formats.OBJ;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace nio2so.TSOView2.Formats.Terrain
{   
    /// <summary>
    /// Interaction logic for TSOCityViewPage.xaml
    /// </summary>
    public partial class TSOCityViewPage : Page
    {
        TSOCity CurrentCity => CityTerrainHandler.Current.City;
        TSOCityMesh CurrentMesh => CityTerrainHandler.Current.Mesh;

        public TSOCityViewPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => LoadMap();

        private void LoadMap()
        {
            IEnumerable<GeometryModel3D> models = CurrentMesh.To3DGeometry();
            ModelViewer.SetObjects([.. models]);
        }
    }
}

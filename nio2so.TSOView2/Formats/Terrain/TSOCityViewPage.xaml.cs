using nio2so.Formats.Terrain;
using nio2so.TSOView2.Formats.OBJ;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using WpfHexaEditor.Core.MethodExtention;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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

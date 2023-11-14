using nio2so.Formats.Terrain;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).PreviewKeyDown += UserKeyboardInput;

            LoadMap();
        }

        private void LoadMap()
        {
            // CLEAR 3D SCENE
            MainSceneGroup.Children.Clear();

            foreach (var child3D in CurrentMesh.To3DGeometry())
                MainSceneGroup.Children.Add(child3D);
        }

        private void UserKeyboardInput(object sender, KeyEventArgs e)
        {
            Camera.MoveBy(e.Key, 10).RotateBy(e.Key, 3);
        }

        Point from;
        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            var till = e.GetPosition(sender as IInputElement);
            double dx = till.X - from.X;
            double dy = till.Y - from.Y;
            from = till;

            var distance = dx * dx + dy * dy;
            if (distance <= 0d)
                return;

            if (e.MouseDevice.LeftButton is MouseButtonState.Pressed)
            {
                var angle = (distance / Camera.FieldOfView) % 45d;
                Camera.Rotate(new(dy, -dx, 0d), angle);
            }
        }
    }
}

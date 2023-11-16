using nio2so.Formats.Terrain;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.Terrain
{   
    /// <summary>
    /// Interaction logic for TSOCityViewPage.xaml
    /// </summary>
    public partial class TSOCityViewPage : Page
    {
        private record CameraSettings(Point3D Position,Vector3D LookAt, int Width);

        static Dictionary<int, CameraSettings> settings = new()
        {
            { 0, new(new(0,132,256), new(0.5,-0.5,-0.5),140) },
            { 1, new(new(142,110,301),new(-.08,-.65,-.75),400) }
        };

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
            Window.GetWindow(this).MouseWheel += CityView_MouseWheel;

            LoadMap();
        }

        private void LoadMap()
        {
            // CLEAR 3D SCENE
            MainSceneGroup.Children.Clear();

            foreach (var child3D in CurrentMesh.To3DGeometry())
                MainSceneGroup.Children.Add(child3D);

            //OrthoSetCam(settings[0]);
        }

        private void OrthoSetCam(CameraSettings settings)
        {
            Camera.Width = settings.Width;
            Camera.Position = settings.Position;
            Camera.LookDirection = settings.LookAt;
        }

        private void OnCameraMoved()
        {
            DebugText.Text = $"CAMERA====\nPos: {Camera.Position}\n Look: {Camera.LookDirection}";
        }

        private void UserKeyboardInput(object sender, KeyEventArgs e)
        {
            bool handled = false;
            if (Camera is PerspectiveCamera)
                PERSPECTIVE_HandleKeyboard(e, ref handled);
            else if (Camera is OrthographicCamera)
                ORTHO_HandleKeyboard(e, ref handled);
            if (!handled) return;

            OnCameraMoved();
        }

        Point from;
        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            bool handled = false;
            if (Camera is PerspectiveCamera)
                PERSPECTIVE_HandleMouse(sender, e, ref handled);
            else if (Camera is OrthographicCamera)
                ORTHO_HandleMouse(sender, e, ref handled);
            if (!handled) return;

            OnCameraMoved();
        }

        private void PERSPECTIVE_HandleMouse(object sender, MouseEventArgs e, ref bool Handled)
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
                var fov = 1;// Camera.FieldOfView;
                var angle = (distance / fov) % 45d;
                Camera.Rotate(new(dy, -dx, 0d), angle);
                OnCameraMoved();
            }

            Handled = true;
        }
        private void PERSPECTIVE_HandleKeyboard(KeyEventArgs e, ref bool Handled)
        {
            Camera.MoveBy(e.Key, 10).RotateBy(e.Key, 3);
            Handled = true;
        }

        private void ORTHO_HandleMouse(object sender, MouseEventArgs e, ref bool Handled)
        {
            var till = e.GetPosition(sender as IInputElement);
            double dx = till.X - from.X;
            double dy = till.Y - from.Y;
            from = till;

            var distance = dx * dx + dy * dy;
            if (distance <= 0d)
                return;            

            if (e.MouseDevice.RightButton is MouseButtonState.Pressed)            
                Camera.Position = new Point3D(Camera.Position.X - dx, Camera.Position.Y + dy, Camera.Position.Z + dx);            

            Handled = true;
        }
        private void ORTHO_HandleKeyboard(KeyEventArgs e, ref bool Handled)
        {
            Handled = false;
            ; // placeholder
        }

        private void TSOViewButton_Click(object sender, RoutedEventArgs e)
        {
            OrthoSetCam(settings[0]);
        }

        private void TrainsetViewButton_Click(object sender, RoutedEventArgs e)
        {
            OrthoSetCam(settings[1]);
        }

        private void CityView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Camera is PerspectiveCamera) return;
            (Camera as OrthographicCamera).Width += (e.Delta/5);
            if ((Camera as OrthographicCamera).Width <= 0)
                (Camera as OrthographicCamera).Width = 100;
        }
    }
}

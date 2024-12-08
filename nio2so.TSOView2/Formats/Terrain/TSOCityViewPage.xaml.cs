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
using System.Windows.Media.Animation;
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
        Camera currentCamera => Camera;

        private record CameraSettings(Point3D Position,Vector3D LookAt, int Width);

        /// <summary>
        /// Camera presets for Orthographic camera
        /// </summary>
        static Dictionary<int, CameraSettings> CameraPresets = new()
        {
            { 0, new(new(0,132,256), new(0.5,0.5,0.5),140) },
            { 1, new(new(0,-256,0),new(.5,.5,.5),140) }
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
            ortho_targetWidth = (currentCamera as OrthographicCamera)?.Width ?? 0;

            // CLEAR 3D SCENE
            MainSceneGroup.Children.Clear();

            foreach (var child3D in CurrentMesh.To3DGeometry())
                MainSceneGroup.Children.Add(child3D);

            //OrthoSetCam(settings[0]);
        }

        private void OrthoSetCam(CameraSettings settings)
        {
            var cam = currentCamera as OrthographicCamera;
            cam.Width = settings.Width;
            cam.Position = settings.Position;
            cam.LookDirection = settings.LookAt;
        }

        private void OnCameraMoved()
        {
            DebugText.Text = $"CAMERA====\nPos: {Camera.Position}\n Look: {Camera.LookDirection}";
        }

        private void UserKeyboardInput(object sender, KeyEventArgs e)
        {
            bool handled = false;
            if (currentCamera is PerspectiveCamera)
                PERSPECTIVE_HandleKeyboard(e, ref handled);
            else if (currentCamera is OrthographicCamera)
                ORTHO_HandleKeyboard(e, ref handled);
            if (!handled) return;

            OnCameraMoved();
        }

        Point from;
        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            bool handled = false;
            if (currentCamera is PerspectiveCamera)
                PERSPECTIVE_HandleMouse(sender, e, ref handled);
            else if (currentCamera is OrthographicCamera)
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

        double ortho_targetWidth = 0;

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
        }

        private void TSOViewButton_Click(object sender, RoutedEventArgs e)
        {
            int Width = 1024;
            Vector3D pos = new(4 * (Width / 4), -(Width / 2), 0);
            Vector3D lookAt = new(256, 0, 256);
            var dir = (lookAt - pos);
            dir.Normalize();
            OrthoSetCam(new(new Point3D(pos.X, pos.Y, pos.Z), dir, Width));
        }

        private void TrainsetViewButton_Click(object sender, RoutedEventArgs e)
        {
            OrthoSetCam(CameraPresets[1]);
        }

        private void CityView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (currentCamera is PerspectiveCamera) return;
            ortho_targetWidth += (-e.Delta/5);
            if (ortho_targetWidth <= 0)
                ortho_targetWidth = 20; // bounce effect
            currentCamera.BeginAnimation(OrthographicCamera.WidthProperty, new DoubleAnimation(ortho_targetWidth, TimeSpan.FromSeconds(.5))
            {
                AccelerationRatio = .25,
                DecelerationRatio = .75
            });
        }
    }
}

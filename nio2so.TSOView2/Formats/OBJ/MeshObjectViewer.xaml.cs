using nio2so.Formats.Terrain;
using nio2so.TSOView2.Formats.Terrain;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.OBJ
{
    /// <summary>
    /// Displays a 3D model in a viewport and allows the user to move the camera around to view it from different angles
    /// <para/> Specializes in terrain rendering, but can be used for any 3D model. Uses an orthographic camera.
    /// </summary>
    public partial class MeshObjectViewer : UserControl
    {
        Camera currentCamera => Camera;

        const double MAX_ZOOM = 5;
        const double USER_MAX_ZOOM = 20;

        /// <summary>
        /// Creates a new <see cref="MeshObjectViewer"/> instance"/>
        /// </summary>
        public MeshObjectViewer()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).PreviewKeyDown += UserKeyboardInput;
            Window.GetWindow(this).MouseWheel += CityView_MouseWheel;
        }

        /// <summary>
        /// Clears the 3D Object Canvas and places the new collection at the origin of the scene, with lighting, and a rotating animation.
        /// <para/>This will also set the camera to look at the middle center of the object, with an appropriate viewing distance applied.
        /// </summary>
        /// <param name="Models"></param>
        public void SetObjects(Model3DCollection Models)
        {
            ortho_targetWidth = (currentCamera as OrthographicCamera)?.Width ?? 0;

            // CLEAR 3D SCENE
            MainSceneGroup.Children.Clear();

            if (Models == null || Models.Count() < 1) return;

            //BUILD 3D SCENE
            foreach (var child3D in Models)
                MainSceneGroup.Children.Add(child3D);

            SetCamSettings();
        }

        private void SetCamSettings()
        {
            //**set camera parameters

            if (MainSceneGroup.Children.Count < 1) return;

            Rect3D bounds = MainSceneGroup.Children.OrderByDescending(x => WPF3DExtensions.Area(x.Bounds.Size)).First().Bounds;

            //set position to center
            Camera.Position = WPF3DExtensions.GetAppropriateViewingDistancePosition(
                bounds, currentCamera is PerspectiveCamera pCam ? pCam.FieldOfView : 40, 1.5);

            //look at the ground
            Camera.LookDirection = WPF3DExtensions.GetLookAt3D(Camera.Position, bounds);
            Camera.UpDirection = new Vector3D(0, 1, 0);

            //set the rotation animation
            WPF3DExtensions.SetRotation3DAnimation(this, MainSceneGroup, bounds, TimeSpan.FromSeconds(30));

            Camera.Width = 140;

            PauseAnimationButton.Visibility = Visibility.Visible;
        }

        private void OnCameraMoving(Point3D NewPosition, Point3D? OldPosition, bool WithAnimation = true)
        {
            DebugText.Text = $"Camera Information\n\nPosition: {Camera.Position}\nLook Direction: {Camera.LookDirection}\n";

            //** lerp animation 

            if (WithAnimation)
            {
                var animation = new Point3DAnimation(NewPosition, TimeSpan.FromSeconds(.15))
                {
                    AccelerationRatio = .25,
                    DecelerationRatio = .75,
                    FillBehavior = FillBehavior.Stop
                };
                animation.Completed += delegate
                {
                    Camera.Position = NewPosition;
                };
                currentCamera.BeginAnimation(OrthographicCamera.PositionProperty, animation, HandoffBehavior.SnapshotAndReplace);

            }
            else Camera.Position = NewPosition;
        }

        private void UserKeyboardInput(object sender, KeyEventArgs e)
        {
            bool handled = false;
            Point3D? NewPosition = null;

            if (currentCamera is PerspectiveCamera)
                throw new NotImplementedException("Perspective Camera support has been removed.");
            else if (currentCamera is OrthographicCamera)
            {
                handled = OrthoHandleKeyboard(sender, e, out Point3D fooPosition);
                NewPosition = fooPosition; // nullables are a pain lol
            }
            if (!handled || !NewPosition.HasValue) return;

            OnCameraMoving(NewPosition.Value, currentCamera is OrthographicCamera ortho ? ortho.Position : (currentCamera as PerspectiveCamera)?.Position);
        }

        Point from;
        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            bool handled = false;
            Point3D? NewPosition = null;

            if (currentCamera is PerspectiveCamera)
                throw new NotImplementedException("Perspective Camera support has been removed.");
            else if (currentCamera is OrthographicCamera)
            {
                handled = OrthoHandleMouse(sender, e, out Point3D fooPosition);
                NewPosition = fooPosition; // nullables are a pain lol
            }
            if (!handled || !NewPosition.HasValue) return;

            OnCameraMoving(NewPosition.Value,
                currentCamera is OrthographicCamera ortho ? ortho.Position : (currentCamera as PerspectiveCamera)?.Position,
                false); // mouse movement is too fast for animation system
        }

        double ortho_targetWidth = 0;

        private bool OrthoHandleMouse(object sender, MouseEventArgs e, out Point3D NewPosition)
        {
            var newPos = e.GetPosition(sender as IInputElement);

            if (e.MouseDevice.LeftButton == MouseButtonState.Released)
            {
                from = newPos;
                return false;
            }

            double sensitivity = 1.0;

            double dx = ((newPos.X - from.X) / RenderSize.Width) * Camera.Width;
            double dy = ((newPos.Y - from.Y) / RenderSize.Height) * (Camera.Width / (RenderSize.Width / RenderSize.Height));
            from = newPos;

            var distance = dx * dx + dy * dy;
            if (distance <= 0d)
                return false;

            double frameSpeed = sensitivity * Camera.Width;

            NewPosition = new Point3D(Camera.Position.X - dx, Camera.Position.Y, Camera.Position.Z - dy);
            return true;
        }

        private bool OrthoHandleKeyboard(object sender, KeyEventArgs e, out Point3D NewPosition)
        {
            double pointNudge = 10;

            NewPosition = e.Key switch
            {
                Key.W => new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - pointNudge),
                Key.S => new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + pointNudge),
                Key.A => new Point3D(Camera.Position.X - pointNudge, Camera.Position.Y, Camera.Position.Z),
                Key.D => new Point3D(Camera.Position.X + pointNudge, Camera.Position.Y, Camera.Position.Z),
                _ => (NewPosition = default)
            };

            return true;
        }

        private void ResetCameraButton_Click(object sender, RoutedEventArgs e) => SetCamSettings();

        private void PauseAnimationButton_Click(object sender, RoutedEventArgs e)
        {
            WPF3DExtensions.StopRotation3DAnimation(this);
            PauseAnimationButton.Visibility = Visibility.Collapsed;
        }

        private void RecenterButton_Click(object sender, RoutedEventArgs e)
        {
            Rect3D bounds = MainSceneGroup.Children.OrderByDescending(x => WPF3DExtensions.Area(x.Bounds.Size)).First().Bounds;
            //look at the ground
            Camera.LookDirection = WPF3DExtensions.GetLookAt3D(Camera.Position, bounds);
            Camera.UpDirection = new Vector3D(0, 1, 0);
        }

        private void CityView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (currentCamera is PerspectiveCamera) throw new NotImplementedException("Perspective Camera support has been removed.");

            double newZoom = ortho_targetWidth + (-e.Delta / 5);
            OnCameraZooming(newZoom);
        }

        private void OnCameraZooming(double NewZoomWidth)
        {
            ortho_targetWidth = NewZoomWidth;

            if (ortho_targetWidth <= MAX_ZOOM)
                ortho_targetWidth = MAX_ZOOM;

            var animation = new DoubleAnimation(ortho_targetWidth, TimeSpan.FromSeconds(.5))
            {
                AccelerationRatio = .25,
                DecelerationRatio = .75,
                FillBehavior = FillBehavior.Stop
            };
            animation.Completed += delegate
            {
                Camera.Width = ortho_targetWidth;
                if (ortho_targetWidth < USER_MAX_ZOOM) // bounce effect
                    OnCameraZooming(USER_MAX_ZOOM);
            };
            currentCamera.BeginAnimation(OrthographicCamera.WidthProperty, animation, HandoffBehavior.SnapshotAndReplace);
        }
    }
}

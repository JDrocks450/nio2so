using System.Windows.Input;
using System.Windows.Media.Media3D;

using static System.Windows.Input.Key;
using static System.Windows.Input.ModifierKeys;

namespace nio2so.TSOView2.Util
{
    public static class ProjectionCameraExtensions
    {
        public static TCamera Move<TCamera>(this TCamera camera, Vector3D axis, double step)
            where TCamera : ProjectionCamera
        {
            camera.Position += axis * step;
            return camera;
        }

        public static TCamera Rotate<TCamera>(this TCamera camera, Vector3D axis, double angle)
            where TCamera : ProjectionCamera
        {
            Matrix3D matrix3D = new();
            matrix3D.RotateAt(new(axis, angle), camera.Position);
            camera.LookDirection *= matrix3D;
            return camera;
        }

        public static Vector3D GetYawAxis(this ProjectionCamera camera) => camera.UpDirection;
        public static Vector3D GetRollAxis(this ProjectionCamera camera) => camera.LookDirection;
        public static Vector3D GetPitchAxis(this ProjectionCamera camera) => Vector3D.CrossProduct(camera.UpDirection, camera.LookDirection);

        public static PerspectiveCamera MoveBy(this PerspectiveCamera camera, Key key) => camera.MoveBy(key, camera.FieldOfView / 180d);
        public static PerspectiveCamera RotateBy(this PerspectiveCamera camera, Key key) => camera.RotateBy(key, camera.FieldOfView / 45d);

        public static TCamera MoveBy<TCamera>(this TCamera camera, Key key, double step) where TCamera : ProjectionCamera => key switch
        {
            W => camera.Move(Keyboard.Modifiers.HasFlag(Shift) ? camera.GetYawAxis() : camera.GetRollAxis(), +step),
            S => camera.Move(Keyboard.Modifiers.HasFlag(Shift) ? camera.GetYawAxis() : camera.GetRollAxis(), -step),
            A => camera.Move(camera.GetPitchAxis(), +step),
            D => camera.Move(camera.GetPitchAxis(), -step),

            _ => camera
        };

        public static TCamera RotateBy<TCamera>(this TCamera camera, Key key, double angle) where TCamera : ProjectionCamera => key switch
        {
            Left => camera.Rotate(camera.GetYawAxis(), +angle),
            Right => camera.Rotate(camera.GetYawAxis(), -angle),
            Down => camera.Rotate(camera.GetPitchAxis(), +angle),
            Up => camera.Rotate(camera.GetPitchAxis(), -angle),

            _ => camera
        };
    }
}

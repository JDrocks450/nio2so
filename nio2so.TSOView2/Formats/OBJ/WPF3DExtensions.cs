using nio2so.Data.Common;
using nio2so.Formats.Terrain;
using nio2so.TSOView2.Formats.Terrain;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace nio2so.TSOView2.Formats.OBJ;

/// <summary>
/// Extensions for working with <c>nio2so.Formats.Terrain</c> objects
/// </summary>
internal static class WPF3DExtensions
{
    const double VertexColorStrength = 1;

    public static Vector3D ToVector3D(this GeomVector3 Point) => new(Point.X, Point.Y, Point.Z);
    public static Point3D ToPoint3D(this GeomVector3 Point) => new(Point.X, Point.Y, Point.Z);
    public static Point ToPoint2D(this GeomPoint Point) => new(Point.X, Point.Y);
    public static Color ToMediaColor(this System.Drawing.Color MColor) => Color.FromArgb(MColor.A, MColor.R, MColor.G, MColor.B);
    public static Brush? ToMediaBrush(this TSOCityBrush brush, TSOCityContentManager Manager)
    {
        if (!brush.TryGetTextureRef(Manager, out var resource)) return default;
        var managedRes = resource.Convert();
        return new ImageBrush()
        {
            ImageSource = managedRes
        };
    }

    /// <summary>
    /// See: <see cref="TSOCityGeom.GenerateMeshGeometry(TSOCity)"/>
    /// <para/><seealso href="https://github.com/riperiperi/FreeSO"/>
    /// </summary>
    /// <param name="City"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static Task<Vector3D> GetNormalAt(TSOCity City, int x, int y)
    {
        return Task.Factory.StartNew(delegate
        {
            var sum = new Vector3();
            var rotToNormalXY = Matrix4x4.CreateRotationZ((float)(Math.PI / 2));
            var rotToNormalZY = Matrix4x4.CreateRotationX(-(float)(Math.PI / 2));
            var reso = City.CityDataResolution;

            if (x < reso.X - 1)
            {
                var vec = new Vector3();
                vec.X = 1;
                vec.Y = City.GetElevationPoint(x + 1, y) - City.GetElevationPoint(x, y);
                vec = Vector3.Transform(vec, rotToNormalXY);
                sum += vec;
            }

            if (x > 1)
            {
                var vec = new Vector3();
                vec.X = 1;
                vec.Y = City.GetElevationPoint(x, y) - City.GetElevationPoint(x - 1, y);
                vec = Vector3.Transform(vec, rotToNormalXY);
                sum += vec;
            }

            if (y < reso.Y - 1)
            {
                var vec = new Vector3();
                vec.Z = 1;
                vec.Y = City.GetElevationPoint(x, y + 1) - City.GetElevationPoint(x, y);
                vec = Vector3.Transform(vec, rotToNormalZY);
                sum += vec;
            }

            if (y > 1)
            {
                var vec = new Vector3();
                vec.Z = 1;
                vec.Y = City.GetElevationPoint(x, y) - City.GetElevationPoint(x, y - 1);
                vec = Vector3.Transform(vec, rotToNormalZY);
                sum += vec;
            }
            if (sum != Vector3.Zero) sum = Vector3.Normalize(sum);
            return new Vector3D(sum.X, -sum.Y, sum.Z);
        });
    }
    /// <summary>
    /// Initializes the <see cref="GeomVertex.Normal"/> property as they come uninitialized
    /// </summary>
    /// <param name="Mesh"></param>
    public static async Task CalculateNormals(this TSOCityMesh Mesh, TSOCity City)
    {
        foreach(var vertCollection in Mesh.Vertices)
        {
            foreach(var vert in vertCollection.Value)
            {
                var mapPos = vert.MapPosition;
                var normal = await GetNormalAt(City, (int)mapPos.X, (int)mapPos.Y);
                vert.Normal = new(normal.X, normal.Y, normal.Z);
            }
        }
    }

    public static Point3D CenterBounds(Rect3D bounds) => new Point3D(
            bounds.X + bounds.SizeX / 2,
            bounds.Y + bounds.SizeY / 2,
            bounds.Z + bounds.SizeZ / 2
        );
    public static double Area(Size3D bounds) => bounds.X * bounds.Y * bounds.Z;                

    public static Point3D GetAppropriateViewingDistancePosition(Rect3D ObjectBoundaries, double FOV, double DistanceMultiplier = 1.0)
    {
        // calculate the distance required to fit the object in view
        double maxDimension = Math.Max(ObjectBoundaries.SizeX, ObjectBoundaries.SizeY);

        // Convert FOV to radians for Math.Tan
        double fovRadians = FOV * (Math.PI / 180);

        // Distance formula: (half-size) / tan(half-fov)
        double distance = maxDimension / 2 / Math.Tan(fovRadians / 2);

        // add margin to screen edges
        distance *= DistanceMultiplier;

        // calculate the center point
        Point3D center = CenterBounds(ObjectBoundaries);

        return new Point3D(center.X, center.Y + distance, center.Z + distance);
    }

    public static Vector3D GetLookAt3D(Point3D CameraPosition, Rect3D ObjectBoundaries)
    {            
        // set model boundaries var
        Rect3D bounds = ObjectBoundaries;

        // calculate the center point
        Point3D center = CenterBounds(bounds);            

        var targetPoint = center;

        // The vector from the camera's current position to the target
        Vector3D newLookDir = new Vector3D(
            targetPoint.X - CameraPosition.X,
            targetPoint.Y - CameraPosition.Y,
            targetPoint.Z - CameraPosition.Z
        );

        return newLookDir;            
    }

    /// <summary>
    /// Applies a rotation animation to the main scene
    /// </summary>
    public static Storyboard? SetRotation3DAnimation(FrameworkElement Parent, Model3DGroup Target, Rect3D ObjectBoundaries, TimeSpan RotationTime, 
        double FromValue = 0, double ToValue = 360, Vector3D? ObjectScale = null, RepeatBehavior? RepeatBehavior = null)
    {
        Storyboard? SceneAnimation = Parent.FindName("SceneRotationAnimation") as Storyboard;

        if (SceneAnimation != default) return null;
        if (!ObjectScale.HasValue) ObjectScale = new Vector3D(1, 1, 1);
        if (!RepeatBehavior.HasValue) RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;

        Point3D center = CenterBounds(ObjectBoundaries);

        SceneAnimation = new Storyboard()
        {
            RepeatBehavior = RepeatBehavior.Value
        };
        var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
        Parent.RegisterName("SceneRotation", rotation);
        var transformGroup = new Transform3DGroup();
        transformGroup.Children.Add(new RotateTransform3D()
        {
            Rotation = rotation,
            CenterX = center.X,
            CenterY = center.Y,
            CenterZ = center.Z
        });
        transformGroup.Children.Add(new ScaleTransform3D(ObjectScale.Value,center));
        var animation = new DoubleAnimation(FromValue, ToValue, RotationTime);
        SceneAnimation.Children.Add(animation);
        Storyboard.SetTargetName(animation, "SceneRotation");
        Storyboard.SetTargetProperty(animation, new PropertyPath(AxisAngleRotation3D.AngleProperty));
        Target.Transform = transformGroup;

        SceneAnimation.Begin(Parent, true);
        Parent.RegisterName("SceneRotationAnimation", SceneAnimation);

        return SceneAnimation;
    }
    public static void StopRotation3DAnimation(FrameworkElement Parent)
    {
        Storyboard? SceneAnimation = Parent.FindName("SceneRotationAnimation") as Storyboard;
        if (SceneAnimation == default) return;
        SceneAnimation.Stop(Parent);
        Parent.UnregisterName("SceneRotation");
        Parent.UnregisterName("SceneRotationAnimation");
    }

    /// <summary>
    /// Converts the given <see cref="TSOCityMesh"/> into a WPF Media3D <see cref="MeshGeometry3D"/>
    /// <para>See also: <seealso cref="To3DGeometry(TSOCityMesh)"/></para>
    /// </summary>
    /// <param name="Mesh">Please call <see cref="FixNormals(TSOCityMesh)"/> before calling this method.</param>
    /// <param name="TerrainType"></param>
    /// <param name="Debug_VertexColor"></param>
    /// <returns></returns>
    public static MeshGeometry3D ToMeshGeometry(this TSOCityMesh Mesh, TSOCityTerrainTypes TerrainType, out Color Debug_VertexColor)
    {            
        MeshGeometry3D meshGeometry = new MeshGeometry3D()
        {
            Positions = new Point3DCollection(Mesh.Vertices[(int)TerrainType].Select(x => x.Position.ToPoint3D())),
            TriangleIndices = new Int32Collection(Mesh.Indices[(int)TerrainType]),
            Normals = new Vector3DCollection(Mesh.Vertices[(int)TerrainType].Select(x => x.Normal.ToVector3D())),
            TextureCoordinates = new PointCollection(Mesh.Vertices[(int)TerrainType].Select(x => x.TextureCoord.ToPoint2D()))
        };
        Debug_VertexColor = Mesh.Vertices[(int)TerrainType][0].Debug_TileColorHilight.ToMediaColor();
        return meshGeometry;
    }
    /// <summary>
    /// Converts the given <see cref="TSOCityMesh"/> into a WPF Media3D <see cref="MeshGeometry3D"/>
    /// <para>See also: <seealso cref="To3DGeometry(TSOCityMesh)"/></para>
    /// </summary>
    /// <param name="Mesh">Please call <see cref="FixNormals(TSOCityMesh)"/> before calling this method.</param>
    /// <param name="TerrainType"></param>
    /// <param name="Debug_VertexColor"></param>
    /// <returns></returns>
    public static MeshGeometry3D ToVertexColorMeshGeometry(this TSOCityMesh Mesh, out Material VertexColorMaterial)
    {
        var flattenedVerts = Mesh.Vertices[155];
        var indices = Mesh.Indices[155];

        MeshGeometry3D meshGeometry = new MeshGeometry3D()
        {
            Positions = new Point3DCollection(flattenedVerts.Select(y => y.Position.ToPoint3D())),
            TriangleIndices = new Int32Collection(indices),
            Normals = new Vector3DCollection(flattenedVerts.Select(y => y.Normal.ToVector3D())),
            TextureCoordinates = new PointCollection(flattenedVerts.Select(y => y.VertexColorAtlasCoord.ToPoint2D()))
        };
        //set texture of vertex color atlas
        var managedRes = Mesh.VertexColorAtlas.Convert();
        var brush = new ImageBrush()
        {
            ImageSource = managedRes,
            Opacity = VertexColorStrength
        };
        VertexColorMaterial = new DiffuseMaterial(brush);            
        return meshGeometry;
    }
    /// <summary>
    /// Converts the given <see cref="TSOCityBrushGeometry"/> into a WPF Media3D <see cref="MeshGeometry3D"/>
    /// <para>See also: <seealso cref="To3DGeometry(TSOCityMesh)"/></para>
    /// </summary>
    /// <param name="Mesh">Please call <see cref="FixNormals(TSOCityMesh)"/> before calling this method.</param>
    /// <param name="TerrainType"></param>
    /// <param name="Debug_VertexColor"></param>
    /// <returns></returns>
    public static MeshGeometry3D ToMeshGeometry(this TSOCityBrushGeometry Mesh)
    {
        MeshGeometry3D meshGeometry = new MeshGeometry3D()
        {
            Positions = new Point3DCollection(Mesh.Vertices.Select(x => x.Position.ToPoint3D())),
            TriangleIndices = new Int32Collection(Mesh.Indices),
            Normals = new Vector3DCollection(Mesh.Vertices.Select(x => x.Normal.ToVector3D())),
            TextureCoordinates = new PointCollection(Mesh.Vertices.Select(x => x.TextureCoord.ToPoint2D()))
        };
        return meshGeometry;
    }
    /// <summary>
    /// Converts the given <see cref="TSOCityMesh"/> into a WPF Media3D <see cref="Geometry3D"/>
    /// </summary>
    /// <param name="Mesh">Please call <see cref="FixNormals(TSOCityMesh)"/> before calling this method.</param>
    /// <returns></returns>
    public static IEnumerable<GeometryModel3D> To3DGeometry(this TSOCityMesh Mesh)
    {
        List<GeometryModel3D> models = new();
        var backBrush = new DiffuseMaterial(Brushes.Black);

        //brush geometry
        foreach (var brushMeshKeyValue in Mesh.BrushGeometry)
        {
            //get brush
            Brush? brush = brushMeshKeyValue.Key.ToMediaBrush(CityTerrainHandler.Current.ContentManager);
            if (brush == null) brush = new SolidColorBrush(Colors.Black);
            Material material = new DiffuseMaterial(brush);

            MaterialGroup group = new MaterialGroup();
            group.Children.Add(material);

            //get mesh geometry
            MeshGeometry3D meshGeom = brushMeshKeyValue.Value.ToMeshGeometry();

            GeometryModel3D geom = new()
            {
                Geometry = meshGeom,
                Material = group,
                BackMaterial = group
            };
            models.Add(geom);
        }

        //vertex colors
        var vertexColorMesh = Mesh.ToVertexColorMeshGeometry(out var atlas);
        GeometryModel3D vertColGeom = new()
        {
            Geometry = vertexColorMesh,
            Material = atlas,
            BackMaterial = atlas,
            Transform = new TranslateTransform3D(0, -.000001, 0)
        };
        models.Add(vertColGeom);

        return models;
    }
}

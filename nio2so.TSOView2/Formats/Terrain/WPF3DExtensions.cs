using nio2so.Data.Common;
using nio2so.Formats.Terrain;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace nio2so.TSOView2.Formats.Terrain
{
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
        private static Vector3D GetNormalAt(TSOCity City, int x, int y)
        {
            var sum = new Vector3();
            var rotToNormalXY = Matrix4x4.CreateRotationZ((float)(Math.PI / 2));
            var rotToNormalZY = Matrix4x4.CreateRotationX(-(float)(Math.PI / 2));
            var reso = City.CityDataResolution;

            if (x < reso.X-1)
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

            if (y < reso.Y-1)
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
        }
        /// <summary>
        /// Initializes the <see cref="GeomVertex.Normal"/> property as they come uninitialized
        /// </summary>
        /// <param name="Mesh"></param>
        public static void FixNormals(this TSOCityMesh Mesh, TSOCity City)
        {
            foreach(var vertCollection in Mesh.Vertices)
            {
                foreach(var vert in vertCollection.Value)
                {
                    var mapPos = vert.MapPosition;
                    var normal = GetNormalAt(City, (int)mapPos.X, (int)mapPos.Y);
                    vert.Normal = new(normal.X, normal.Y, normal.Z);
                }
            }
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
                TriangleIndices = new System.Windows.Media.Int32Collection(Mesh.Indices[(int)TerrainType]),
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
                TriangleIndices = new System.Windows.Media.Int32Collection(indices),
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
                TriangleIndices = new System.Windows.Media.Int32Collection(Mesh.Indices),
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
                Transform = new TranslateTransform3D(0,-.000001,0),
                //BackMaterial = testMat
            };
            models.Add(vertColGeom);

            return models;
        }
    }
}

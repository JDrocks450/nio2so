using nio2so.Formats.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static nio2so.Formats.Terrain.TSOCity;

namespace nio2so.TSOView2.Formats.Terrain
{
    /// <summary>
    /// Extensions for working with <c>nio2so.Formats.Terrain</c> objects
    /// </summary>
    internal static class WPF3DExtensions
    {
        public static Vector3D ToVector3D(this GeomVector3 Point) => new(Point.X, Point.Y, Point.Z);
        public static Point3D ToPoint3D(this GeomVector3 Point) => new(Point.X, Point.Y, Point.Z);
        public static Point ToPoint2D(this GeomPoint Point) => new(Point.X, Point.Y);
        public static Color ToMediaColor(this System.Drawing.Color MColor) => Color.FromArgb(MColor.A, MColor.R, MColor.G, MColor.B);
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
            var sum = new Microsoft.Xna.Framework.Vector3();
            var rotToNormalXY = Microsoft.Xna.Framework.Matrix.CreateRotationZ((float)(Math.PI / 2));
            var rotToNormalZY = Microsoft.Xna.Framework.Matrix.CreateRotationX(-(float)(Math.PI / 2));
            var reso = City.CityDataResolution;

            if (x < reso.X-1)
            {
                var vec = new Microsoft.Xna.Framework.Vector3();
                vec.X = 1;
                vec.Y = City.GetElevationPoint(x + 1, y) - City.GetElevationPoint(x, y);
                vec = Microsoft.Xna.Framework.Vector3.Transform(vec, rotToNormalXY);
                sum += vec;
            }

            if (x > 1)
            {
                var vec = new Microsoft.Xna.Framework.Vector3();
                vec.X = 1;
                vec.Y = City.GetElevationPoint(x, y) - City.GetElevationPoint(x - 1, y);
                vec = Microsoft.Xna.Framework.Vector3.Transform(vec, rotToNormalXY);
                sum += vec;
            }

            if (y < reso.Y-1)
            {
                var vec = new Microsoft.Xna.Framework.Vector3();
                vec.Z = 1;
                vec.Y = City.GetElevationPoint(x, y + 1) - City.GetElevationPoint(x, y);
                vec = Microsoft.Xna.Framework.Vector3.Transform(vec, rotToNormalZY);
                sum += vec;
            }

            if (y > 1)
            {
                var vec = new Microsoft.Xna.Framework.Vector3();
                vec.Z = 1;
                vec.Y = City.GetElevationPoint(x, y) - City.GetElevationPoint(x, y - 1);
                vec = Microsoft.Xna.Framework.Vector3.Transform(vec, rotToNormalZY);
                sum += vec;
            }
            if (sum != Microsoft.Xna.Framework.Vector3.Zero) sum.Normalize();
            return new Vector3D(sum.X, sum.Y, sum.Z);
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
                Normals = new Vector3DCollection(Mesh.Vertices[(int)TerrainType].Select(x => x.Normal.ToVector3D()))
            };
            Debug_VertexColor = Mesh.Vertices[(int)TerrainType][0].VertexColor.ToMediaColor();
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

            foreach (var terrainType in Mesh.Vertices)
            {
                //get mesh geometry
                MeshGeometry3D meshGeom = Mesh.ToMeshGeometry((TSOCityTerrainTypes)terrainType.Key, out var vColor);

                //get draw brush
                Brush color = new SolidColorBrush(vColor);
                Material material = new DiffuseMaterial(color);

                GeometryModel3D geom = new()
                {
                    Geometry = meshGeom,
                    Material = backBrush,
                    BackMaterial = material
                };
                models.Add(geom);
            }
            return models;
        }
    }
}

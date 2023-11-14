using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Formats.Terrain.TSOCity;

namespace nio2so.Formats.Terrain
{
    public record GeomPoint(double X, double Y);
    public record GeomVector3(double X, double Y, double Z);
    public class GeomVertex
    {
        public Color VertexColor { get; set; }
        public GeomVector3 Position { get; }
        public GeomPoint MapPosition { get; }
        public GeomVector3 Normal { get; set; }
        public GeomPoint TextureCoord { get; }

        public GeomVertex(GeomVector3 Position, GeomPoint MapPosition, GeomVector3 Normal, GeomPoint TextureCoord)
        {
            this.Position = Position;
            this.MapPosition = MapPosition;
            this.Normal = Normal;
            this.TextureCoord = TextureCoord;
        }
    }

    /// <summary>
    /// A mesh for the city terrain generated using <see cref="TSOCityGeom"/>
    /// </summary>
    public class TSOCityMesh
    {
        /// <summary>
        /// Vertex collections with optional layer functionality        
        /// </summary>
        public Dictionary<int, List<GeomVertex>> Vertices { get; } = new();
        /// <summary>
        /// Vertex Index collections with optional layer functionality        
        /// </summary>
        public Dictionary<int, List<int>> Indices { get; } = new();
    }

    /// <summary>
    /// Handles generating the mesh for the Map View
    /// </summary>
    internal class TSOCityGeom
    {
        public delegate void OnSignalBlendHandler(TSOCity City, int MapX, int MapY, TSOCityTerrainTypes This, TSOCityTerrainTypes[] Edges);   
        public event OnSignalBlendHandler OnSignalBlend;

        /// <summary>
        /// An internally accessible default <see cref="TSOCityGeom"/> instance
        /// </summary>
        public static TSOCityGeom GlobalDefault { get; } = new();

        public TSOCity CurrentCity { get; private set; }

        /// <summary>
        /// Since Math libraries needed to perform this calculation are not included, this function returns a default value.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static GeomVector3 GetNormalAt(int X, int Y)
        {
            return new GeomVector3('l','o','l'); // lol :)
        }

        /// <summary>
        /// Signals the <see cref="OnSignalBlend"/> <see langword="event"/> if hooked.
        /// </summary>
        /// <param name="TerrainTypeData"></param>
        /// <param name="y"></param>
        /// <param name="x"></param>
        private void SignalBlend(UtilImageIndexer<TSOCityTerrainTypes> TerrainTypeData, int y, int x)
        {
            if (OnSignalBlend == null) return; // No one is listening to me
            TSOCityTerrainTypes sample;
            TSOCityTerrainTypes t;

            var edges = new TSOCityTerrainTypes[] { TSOCityTerrainTypes.Nothing, TSOCityTerrainTypes.Nothing,
                TSOCityTerrainTypes.Nothing, TSOCityTerrainTypes.Nothing };

            int imgSize = (int)CurrentCity.CityDataResolution.X;
            sample = TerrainTypeData[y * imgSize + x];
            
            t = TerrainTypeData[Math.Abs((y - 1) * imgSize + x)]; // up
            if ((y - 1 >= 0) && (t > sample) && t != TSOCityTerrainTypes.Nothing) edges[0] = t;
            t = TerrainTypeData[y * imgSize + x + 1]; // right
            if ((x + 1 < imgSize) && (t > sample) && t != TSOCityTerrainTypes.Nothing) edges[1] = t;
            t = TerrainTypeData[Math.Min((y + 1), imgSize-1) * imgSize + x]; // down
            if ((y + 1 < imgSize) && (t > sample) && t != TSOCityTerrainTypes.Nothing) edges[2] = t;
            t = TerrainTypeData[y * imgSize + x - 1]; // left
            if ((x - 1 >= 0) && (t > sample) && t != TSOCityTerrainTypes.Nothing) edges[3] = t;

            OnSignalBlend(CurrentCity, x, y, sample, edges);
        }

        /// <summary>
        /// Generates a <see cref="TSOCityMesh"/> using a modified version of the City rendering algorithm used in FreeSO.
        /// <see href="https://github.com/riperiperi/FreeSO"/>
        /// </summary>
        /// <param name="CityData"></param>
        /// <returns></returns>
        public Task<TSOCityMesh> GenerateMeshGeometry(TSOCity CityData)
        {
            CurrentCity = CityData;

            var ElevationData = CityData.GetElevationMap();
            var TerrainData = CityData.GetTerrainMap();

            //the mesh
            TSOCityMesh mesh = new TSOCityMesh();
            for (int i = 0; i < 6; i++)
            { // layers for each type of terrain
                if (i == 5) i = 255;
                mesh.Vertices.Add(i, new());
                mesh.Indices.Add(i, new());
            }

            return Task.Run(delegate
            {
                int xStart, xEnd;
                int imgSize = (int)(CityData.CityDataResolution?.X ?? 512);

                int chunkSize = 16;

                int yStart = 0, yEnd = imgSize;

                var chunkWidth = imgSize / chunkSize;
                var chunkCount = chunkWidth * chunkWidth;

                var ci = 0;
                for (int cy = 0; cy < chunkWidth; cy++)
                {
                    for (int cx = 0; cx < chunkWidth; cx++)
                    {
                        yStart = cy * chunkSize;
                        yEnd = (cy + 1) * chunkSize;
                        var xLim = cx * chunkSize;
                        var xLimEnd = (cx + 1) * chunkSize;

                        for (int i = yStart; i < yEnd; i++)
                        {
                            //SETTINGS
                            int topLeft = CityData.Settings.TopLeftCornerPosition;
                            int botLeft = CityData.Settings.BottomLeftCornerPosition;
                            double elvScale = CityData.Settings.ElevationScale;

                            //transform image point to map coordinate
                            if (i < topLeft) xStart = topLeft - i;
                            else xStart = i - topLeft;
                            if (i < botLeft) xEnd = (topLeft + 1) + i;
                            else xEnd = imgSize - (i - botLeft);
                            var rXE = xEnd;
                            var rXS = xStart;

                            int rXE2, rXS2;
                            int i2 = i + 1;
                            if (i2 < topLeft) rXS2 = topLeft - i2;
                            else rXS2 = i2 - topLeft;
                            if (i2 < botLeft) rXE2 = (topLeft + 1) + i2;
                            else rXE2 = imgSize - (i2 - botLeft);

                            var fadeRange = 10;
                            var fR = 1 / 9f;
                            xStart = Math.Max(xStart - fadeRange, xLim);
                            xEnd = Math.Min(xLimEnd, xEnd + fadeRange);

                            if (xEnd <= xStart) continue;

                            for (int j = xStart; j < xEnd; j++)
                            { //where the magic happens B)
                                //get terrain type here
                                var ex = Math.Min(Math.Max(rXS, j), rXE - 1);
                                int type = (int)TerrainData[(i * imgSize) + ex];

                                //Get actual Map Position
                                int mapXPos = ex;
                                int mapYPos = i;
                                GeomPoint mapPos = new(mapXPos, mapYPos);

                                SignalBlend(TerrainData, i, ex); //sets the TSO Pre-Alpha

                                //(smaller) segment of code for generating triangles incoming
                                var norm1 = GetNormalAt(Math.Min(rXE, Math.Max(rXS, j)), i);
                                var norm2 = GetNormalAt(Math.Min(rXE, Math.Max(rXS, j + 1)), i);
                                var norm3 = GetNormalAt(Math.Min(rXE2, Math.Max(rXS2, j + 1)), Math.Min((imgSize - 1), i + 1));
                                var norm4 = GetNormalAt(Math.Min(rXE2, Math.Max(rXS2, j)), Math.Min((imgSize - 1), i + 1));
                                //vertex pos
                                var pos1 = new GeomVector3(j, ElevationData[i * imgSize + Math.Min(rXE, Math.Max(rXS, j))], i);
                                var pos2 = new GeomVector3(j + 1, ElevationData[i * imgSize + Math.Min(rXE, Math.Max(rXS, j + 1))], i);
                                var pos3 = new GeomVector3(j + 1, ElevationData[Math.Min(imgSize - 1, i + 1) * imgSize + Math.Min(rXE2, Math.Max(rXS2, j + 1))], i + 1);
                                var pos4 = new GeomVector3(j, ElevationData[Math.Min(imgSize - 1, i + 1) * imgSize + Math.Min(rXE2, Math.Max(rXS2, j))], i + 1);
                                // make geom vert
                                var vert1 = new GeomVertex(pos1, mapPos, norm1, new(0, 0));
                                var vert2 = new GeomVertex(pos2, mapPos, norm2, new(1, 0));
                                var vert3 = new GeomVertex(pos3, mapPos, norm3, new(1, 1));
                                var vert4 = new GeomVertex(pos4, mapPos, norm4, new(0, 1));

                                //set vertex colors
                                vert1.VertexColor = vert2.VertexColor = vert3.VertexColor = vert4.VertexColor =
                                    (TSOCityTerrainTypes)type switch
                                    {
                                        TSOCityTerrainTypes.Nothing => Color.Black,
                                        TSOCityTerrainTypes.Grass => Color.LawnGreen,
                                        TSOCityTerrainTypes.Water => Color.Blue,
                                        TSOCityTerrainTypes.Snow => Color.White,
                                        TSOCityTerrainTypes.Rock => Color.Brown,
                                        TSOCityTerrainTypes.Sand => Color.SandyBrown
                                    };

                                //set verts
                                var baseInd = mesh.Vertices[type].Count;
                                mesh.Vertices[type].Add(vert1);
                                mesh.Vertices[type].Add(vert2);
                                mesh.Vertices[type].Add(vert3);
                                mesh.Vertices[type].Add(vert4);

                                //set square indices
                                mesh.Indices[type].Add(baseInd);
                                mesh.Indices[type].Add(baseInd + 1);
                                mesh.Indices[type].Add(baseInd + 2);
                                mesh.Indices[type].Add(baseInd);
                                mesh.Indices[type].Add(baseInd + 2);
                                mesh.Indices[type].Add(baseInd + 3);
                            }
                        }
                    }
                }
                return mesh;
            });
        }
    }
}

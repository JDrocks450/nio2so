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
        /// <summary>
        /// The color to display when no <see cref="Brush"/> is set
        /// </summary>
        public Color Debug_VertexColor { get; set; }
        public GeomVector3 Position { get; }
        public GeomPoint MapPosition { get; }
        public GeomVector3 Normal { get; set; }
        public GeomPoint TextureCoord { get; }
        /// <summary>
        /// The brush implementers can use which describes how this tile should be drawn
        /// </summary>
        public TSOCityBrush? Brush { get; set; }

        public GeomVertex(GeomVector3 Position, GeomPoint MapPosition, GeomVector3 Normal, GeomPoint TextureCoord)
        {
            this.Position = Position;
            this.MapPosition = MapPosition;
            this.Normal = Normal;
            this.TextureCoord = TextureCoord;
        }
    }

    /// <summary>
    /// A generic way of storing information on how to draw a texture tile
    /// </summary>
    public class TSOCityBrush
    {
        public TSOCityBrush()
        {
        }

        public TSOCityBrush(string TextureName) : this()
        {
            this.TextureName = TextureName;
        }

        /// <summary>
        /// Returns the associated <see cref="TextureName"/> from the provided <see cref="TSOCityContentManager"/>
        /// </summary>
        /// <param name="Manager"></param>
        /// <returns></returns>
        public bool TryGetTextureRef(TSOCityContentManager Manager, out Bitmap? Value)
        {
            Value = default;
            if (Manager.TryGetValue(TextureName.ToLower(), out var imageContent))
                Value = imageContent.ImageReference as Bitmap;
            return Value != default;
        }

        /// <summary>
        /// Instructs the engine displaying the mesh on what texture to use to draw this tile, if applicable in the given context.
        /// </summary>
        public string? TextureName { get; set; }
        public bool HasTexture => TextureName != null;
    }

    /// <summary>
    /// Sub-Meshes that share the same <see cref="TSOCityBrush"/>
    /// </summary>
    public class TSOCityBrushGeometry
    {
        /// <summary>
        /// <see cref="Vertices"/> that share the same <see cref="TSOCityBrush"/>
        /// </summary>
        public List<GeomVertex> Vertices { get; } = new();
        /// <summary>
        /// Vertex Indices that share the same <see cref="TSOCityBrush"/>
        /// </summary>
        public List<int> Indices { get; } = new();
    }

    /// <summary>
    /// A mesh for the city terrain generated using <see cref="TSOCityGeom"/>
    /// </summary>
    public class TSOCityMesh
    {
        public TSOCityMesh()
        {
            UseBrushGeometry = true;
        }
        /// <summary>
        /// This mesh is marked to not use the <see cref="BrushGeometry"/> property
        /// </summary>
        /// <param name="UseBrushGeometry"></param>
        public TSOCityMesh(bool UseBrushGeometry) : this()
        {
            this.UseBrushGeometry = UseBrushGeometry;
        }

        public void InitializeCollections()
        {
            for (int i = 0; i < 6; i++)
            { // layers for each type of terrain
                if (i == 5) i = 255;
                Vertices.Add(i, new());
                Indices.Add(i, new());               
            }
        }

        /// <summary>
        /// Collections of Vertices grouped by their <see cref="TSOCityTerrainTypes"/> value    
        /// </summary>
        public Dictionary<int, List<GeomVertex>> Vertices { get; } = new();
        /// <summary>
        /// Collections of Vertex Indices grouped by their <see cref="TSOCityTerrainTypes"/> value    
        /// </summary>
        public Dictionary<int, List<int>> Indices { get; } = new();
        /// <summary>
        /// Groups the Vertices and Indices together by their shared <see cref="TSOCityBrush"/>
        /// </summary>
        public Dictionary<TSOCityBrush, TSOCityBrushGeometry> BrushGeometry { get; } = new();
        public bool UseBrushGeometry { get; }

        /// <summary>
        /// Maps the <see cref="GeomVertex"/> and optionally its brush to the <see cref="Vertices"/> property and <see cref="BrushGeometry"/>
        /// </summary>
        /// <param name="Layer"></param>
        /// <param name="Vertex"></param>
        public void AddVertex(int Layer, GeomVertex Vertex)
        {
            //add to overall mesh
            Vertices[Layer].Add(Vertex);
            //add to brush geom if applicable
            if (!UseBrushGeometry || Vertex.Brush == default) 
                return;
            var geomLayer = BrushGeometry;
            if(geomLayer == null) 
                return;
            if (!geomLayer.TryGetValue(Vertex.Brush, out var collection))
            {
                collection = new();
                geomLayer.Add(Vertex.Brush, collection);
            }    
            collection.Vertices.Add(Vertex);            
        }
        /// <summary>
        /// Maps the <see cref="GeomVertex"/> Index and optionally its brush to the <see cref="Indices"/> property and <see cref="BrushGeometry"/>
        /// </summary>
        /// <param name="Layer"></param>
        public void AddIndices(int Layer, TSOCityBrush? Brush = default, params int[] IndexIncrements)
        {
            //add to overall mesh
            int baseInd = Vertices[Layer].Count - 4;
            foreach (var index in IndexIncrements)
                Indices[Layer].Add(baseInd + index);
            //add to brush geom if applicable
            if (!UseBrushGeometry || Brush == default) return;
            var geomLayer = BrushGeometry;
            if (geomLayer == null) return;
            if (!geomLayer.TryGetValue(Brush, out var collection)) return;                
            baseInd = collection.Vertices.Count - 4;
            foreach (var index in IndexIncrements)
                collection.Indices.Add(baseInd + index);
        }
    }

    /// <summary>
    /// Handles generating the mesh for the Map View
    /// </summary>
    internal class TSOCityGeom
    {
        public delegate void OnSignalBlendHandler(TSOCity City, int MapX, int MapY, TSOCityTerrainTypes This, TSOCityTerrainTypes Other, int binary, out TSOCityBrush Brush);   
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
        private TSOCityBrush DoSignalBlend(UtilImageIndexer<TSOCityTerrainTypes> TerrainTypeData, int y, int x)
        {
            if (OnSignalBlend == null) return default; // No one is listening to me
            TSOCityTerrainTypes sample;
            TSOCityTerrainTypes t;

            var edges = new int[] { -1, -1,-1, -1 };                

            int imgSize = (int)CurrentCity.CityDataResolution.X;
            sample = TerrainTypeData[y * imgSize + x];
            
            t = TerrainTypeData[Math.Abs((y - 1) * imgSize + x)]; // up
            if ((y - 1 >= 0) && (t < sample) && t != TSOCityTerrainTypes.Nothing) edges[0] = (int)t;
            t = TerrainTypeData[y * imgSize + x + 1]; // right
            if ((x + 1 < imgSize) && (t < sample) && t != TSOCityTerrainTypes.Nothing) edges[1] = (int)t;
            t = TerrainTypeData[Math.Min((y + 1), imgSize-1) * imgSize + x]; // down
            if ((y + 1 < imgSize) && (t < sample) && t != TSOCityTerrainTypes.Nothing) edges[2] = (int)t;
            t = TerrainTypeData[y * imgSize + x - 1]; // left
            if ((x - 1 >= 0) && (t < sample) && t != TSOCityTerrainTypes.Nothing) edges[3] = (int)t;
            /*
            t = TerrainTypeData[Math.Abs((y - 1) * imgSize + x)]; // up
            if ((y - 1 >= 0) && t != TSOCityTerrainTypes.Nothing) edges[0] = t;
            t = TerrainTypeData[y * imgSize + x + 1]; // right
            if ((x + 1 < imgSize) && t != TSOCityTerrainTypes.Nothing) edges[1] = t;
            t = TerrainTypeData[Math.Min((y + 1), imgSize - 1) * imgSize + x]; // down
            if ((y + 1 < imgSize) && t != TSOCityTerrainTypes.Nothing) edges[2] = t;
            t = TerrainTypeData[y * imgSize + x - 1]; // left
            if ((x - 1 >= 0) && t != TSOCityTerrainTypes.Nothing) edges[3] = t;*/

            int binary =
            ((edges[0] > -1) ? 0 : 2) |
            ((edges[1] > -1) ? 0 : 1) |
            ((edges[2] > -1) ? 0 : 8) |
            ((edges[3] > -1) ? 0 : 4);

            int maxEdge = 4;

            for (int i = 0; i < 4; i++)
                if (edges[i] < maxEdge && edges[i] != -1) maxEdge = edges[i];

            OnSignalBlend(CurrentCity, x, y, sample, (TSOCityTerrainTypes)maxEdge, binary, out var brush);
            return brush;
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
            mesh.InitializeCollections();

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

                                //rabbit hole here
                                TSOCityBrush brush = DoSignalBlend(TerrainData, i, ex); //sets the TSO Pre-Alpha texture reference

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

                                //set debug vertex colors
                                vert1.Debug_VertexColor = vert2.Debug_VertexColor = vert3.Debug_VertexColor = vert4.Debug_VertexColor =
                                    (TSOCityTerrainTypes)type switch
                                    {
                                        TSOCityTerrainTypes.Nothing => Color.Black,
                                        TSOCityTerrainTypes.Grass => Color.LawnGreen,
                                        TSOCityTerrainTypes.Water => Color.Blue,
                                        TSOCityTerrainTypes.Snow => Color.White,
                                        TSOCityTerrainTypes.Rock => Color.Brown,
                                        TSOCityTerrainTypes.Sand => Color.SandyBrown
                                    };
                                //set brushes
                                vert1.Brush = vert2.Brush = vert3.Brush = vert4.Brush = brush;

                                //set verts
                                mesh.AddVertex(type, vert1);
                                mesh.AddVertex(type, vert2);
                                mesh.AddVertex(type, vert3);
                                mesh.AddVertex(type, vert4);

                                //set square indices
                                mesh.AddIndices(type, brush, 0, 1, 2, 0, 2, 3);
                            }
                        }
                    }
                }
                return mesh;
            });
        }
    }
}

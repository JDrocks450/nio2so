using nio2so.Data.Common;
using nio2so.Formats.Util;
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

    /// <summary>
    /// Handles generating the mesh for the Map View
    /// </summary>
    internal class TSOCityGeom
    {
        public delegate void OnSignalBlendHandler(TSOCity City, int MapX, int MapY, TSOCityTerrainTypes This, TSOCityTerrainTypes Other, int binary, out TSOCityBrush Brush);   
        public event OnSignalBlendHandler OnTileTextureRequested;

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

        private Color GetVertexColor(UtilImageIndexer VertexColorReader, int y, int x)
        {
            var vertColor = Color.FromArgb(0, 255, 255, 255);
            if (VertexColorReader == null) return vertColor; // transwhite
            int imgSize = VertexColorReader.ImageReference.Width;
            vertColor = VertexColorReader[y * imgSize + x];
            // halloween death world theme
            //vertColor = Color.FromArgb(255, Math.Abs(vertColor.R - 255), Math.Abs(vertColor.G - 255), Math.Abs(vertColor.B - 255));
            // color 2 alpha
            vertColor = vertColor.ColorToAlpha(Color.White);
            return vertColor;
        }

        /// <summary>
        /// Signals the <see cref="OnTileTextureRequested"/> <see langword="event"/> if hooked.
        /// </summary>
        /// <param name="TerrainTypeData"></param>
        /// <param name="y"></param>
        /// <param name="x"></param>
        private TSOCityBrush GetGraphicsTile(UtilImageIndexer<TSOCityTerrainTypes> TerrainTypeData, int y, int x)
        {
            //GET GRAPHICS TILE TEXTURE
            if (OnTileTextureRequested == null) return default; // No one is listening to me
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

            OnTileTextureRequested(CurrentCity, x, y, sample, (TSOCityTerrainTypes)maxEdge, binary, out var brush);
            return brush;
        }

        /// <summary>
        /// Generates a <see cref="TSOCityMesh"/> using a modified version of the City rendering algorithm used in FreeSO.
        /// <see href="https://github.com/riperiperi/FreeSO"/>
        /// </summary>
        /// <param name="CityData"></param>
        /// <returns></returns>
        public Task<TSOCityMesh> GenerateMeshGeometry(TSOCity CityData, bool VertexColorAtlasEnabled = true)
        {
            CurrentCity = CityData;

            var ElevationData = CityData.GetElevationMap();
            var TerrainData = CityData.GetTerrainMap();
            var VertexColorData = CityData.GetVertexColorMap();            

            //the mesh
            TSOCityMesh mesh = new TSOCityMesh();
            mesh.InitializeCollections();

            //make the vertex color atlas
            if (VertexColorAtlasEnabled)            
                mesh.VertexColorAtlas = new Bitmap((int)CityData.CityDataResolution.X, (int)CityData.CityDataResolution.Y, 
                    System.Drawing.Imaging.PixelFormat.Format64bppArgb);                            

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

                            var fadeRange = 0;
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

                                //invoke CityImporter to get the tile texture from its content manager
                                TSOCityBrush brush = GetGraphicsTile(TerrainData, mapYPos, mapXPos); //sets the TSO Pre-Alpha texture reference
                                //invoke the CityImporter to get the vertex color from its content manager
                                Color vertexColor = GetVertexColor(VertexColorData, mapYPos, mapXPos);
                                if (VertexColorAtlasEnabled)
                                    mesh.VertexColorAtlas.SetPixel(mapXPos,mapYPos,vertexColor);

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
                                vert1.Debug_TileColorHilight = vert2.Debug_TileColorHilight = vert3.Debug_TileColorHilight = vert4.Debug_TileColorHilight =
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
                                //set vert colors
                                vert1.VertexColor = vert2.VertexColor = vert3.VertexColor = vert4.VertexColor = vertexColor;
                                vert1.VertexColorAtlasCoord =  new GeomPoint(mapXPos / (float)imgSize, mapYPos / (float)imgSize);
                                vert2.VertexColorAtlasCoord = new GeomPoint((mapXPos + 1) / (float)imgSize, mapYPos / (float)imgSize);
                                vert3.VertexColorAtlasCoord = new GeomPoint((mapXPos + 1) / (float)imgSize, (mapYPos + 1) / (float)imgSize);
                                vert4.VertexColorAtlasCoord = new GeomPoint(mapXPos / (float)imgSize, (mapYPos + 1) / (float)imgSize);

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
                //if (VertexColorAtlasEnabled) mesh.VertexColorAtlas.MakeTransparent(Color.FromArgb(255, 0, 0, 0)); // black
                return mesh;
            });
        }
    }
}

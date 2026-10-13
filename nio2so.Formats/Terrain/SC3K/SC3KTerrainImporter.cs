using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace nio2so.Formats.Terrain.SC3K
{
    /// <summary>
    /// Imports terrain data from a The Sims Online MatchMaker/CityMap.BIN (SimCity 3000) file and converts it into a format that can be used by the TSOView2 application.
    /// <para/><para/>
    /// SC3K Terrain represents the data structure found within the MatchMaker/CityMap.BIN file, which contains terrain information very similar
    /// to that found in the World Data for SimCity 3000.
    /// </summary>
    public class SC3KTerrainImporter
    {
        const string BINFileName = "citymap.bin";
        const string PaletteFileName = "cityterrainpalette.bmp";

        /// <summary>
        /// Major offsets in the CityMap.BIN file
        /// </summary>
        public enum TSOCityBinFileOffsets : long
        {
            Elevation = 0x0,
            ForestMisc = 0x1020D,
            ScreenSpaceUV = 0x3020D //B
        }

        private SC3KTerrainGraphics Graphics;

        public SC3KTerrainDescription TerrainInfo => Graphics.TerrainInfo;

        /// <summary>
        /// Creates a new instance of the <see cref="SC3KTerrainImporter"/> with the supplied <paramref name="MatchMakerDirectory"/>
        /// </summary>
        /// <param name="MatchMakerDirectory">MatchMaker folder in the TSO Directory</param>
        /// <param name="BINFileName"></param>
        /// <param name="PaletteFileName"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public SC3KTerrainImporter(DirectoryInfo MatchMakerDirectory, string BINFileName = BINFileName, string PaletteFileName = PaletteFileName)
        {
            Graphics = new SC3KTerrainGraphics(MatchMakerDirectory.FullName, BINFileName, PaletteFileName);
            if (!Graphics.EnsureAllFiles())
                throw new FileNotFoundException("One or more required files for SC3K terrain import are missing. " +
                    $"Please ensure the following files are present in the ({MatchMakerDirectory.FullName}) directory: "
                    + string.Join(", ", new[] { BINFileName, PaletteFileName }));
        }

        /// <summary>
        /// Returns the elevation data found in the binary file -- this chunk also doubles as supplying terrain colors because of how 
        /// SC3K handles elevation and terrain colors
        /// </summary>
        /// <param name="W">Map width</param>
        /// <param name="H">Map height</param>
        /// <returns></returns>
        public byte[] GetElevationChunk(out int W, out int H) => Graphics.GetElevationChunk(out W, out H);
        /// <summary>
        /// Returns a Greyscale elevation heightmap used to construct a 3D mesh terrain
        /// <para/>
        /// <paramref name="NormalizeHeight"/> if true will set the highest found elevation to map to the max terrain height and scale the 
        /// remaining heights to this value
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderElevationTexture(bool NormalizeHeight = false) => Graphics.RenderElevationTexture(); // greyscale
        /// <summary>
        /// Renders the <see cref="GetElevationChunk(out int, out int)"/> data to a colorized terrain texture using the 
        /// palette supplied in the CityTerrainPalette.BMP file and an SC3K-like color algorithm
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderTerrainTexture() => Graphics.RenderTerrainTexture(); // colorized; special algorithm
        /// <summary>
        /// Returns a Forest Density map and Misc map that is yet to be discovered what it did
        /// </summary>
        /// <returns></returns>
        public Bitmap[] RenderForestMiscTextures() => Graphics.RenderForestMiscChunks();
        /// <summary>
        /// Returns an array of Screen Space Coordinates where each tile on the map is spacially
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public Vector2[] GetScreenSpaceUV(int w = 256, int h = 256) => Graphics.GetScreenSpaceUV(w, h);
        /// <summary>
        /// Returns a scatterplot <see cref="Bitmap"/> containing all the plots from <see cref="GetScreenSpaceUV(int, int)"/>
        /// plus optionally Neighborhoods and their names
        /// </summary>
        /// <returns></returns>
        public Bitmap RenderScreenSpaceUVTexture(bool Neighborhoods = true) => Graphics.RenderScreenSpaceUVTexture(Neighborhoods);
        /// <summary>
        /// Builds a <see cref="TSOSC3KTerrainMesh"/> from the CityBin file, with texture applied
        /// </summary>
        /// <param name="ElevationScale"></param>
        /// <returns></returns>
        public TSOSC3KTerrainMesh BuildTerrainMesh(float ElevationScale = 1.0f) => Graphics.BuildTerrainMesh(ElevationScale);
    }
}

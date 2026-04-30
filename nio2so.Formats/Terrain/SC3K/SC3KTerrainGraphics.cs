using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using static nio2so.Formats.Terrain.SC3K.SC3KTerrainImporter;
using static System.Net.Mime.MediaTypeNames;

namespace nio2so.Formats.Terrain.SC3K
{
    public class SC3KTerrainDescription
    {
        public int WaterLevel
        {
            get;
            set;
        }
        public int TerrainLowestElevation
        {
            get;
            set;
        }
        public int TerrainHighestElevation
        {
            get;
            set;
        }

        public SC3KTerrainDescription(int terrainLowestElevation, int terrainHighestElevation)
        {
            TerrainLowestElevation = terrainLowestElevation;
            TerrainHighestElevation = terrainHighestElevation;
            WaterLevel = TerrainLowestElevation;
        }
    }

    internal class SC3KTerrainGraphics
    {       
        private readonly string WorkingDirectory;

        private readonly Dictionary<string, string> fileNames;

        internal SC3KTerrainDescription TerrainInfo { get; }

        /// <summary>
        /// The level of the water in this terrain
        /// </summary>
        internal int WaterLevel { get => TerrainInfo.WaterLevel; set => TerrainInfo.WaterLevel = value; }

        internal SC3KTerrainGraphics(string workingDirectory, string BINFileName, string PaletteFileName)
        {
            WorkingDirectory = workingDirectory;

            fileNames = new Dictionary<string, string>
            {
                { "BIN", BINFileName },
                { "Palette", PaletteFileName },
                { "Neighborhoods", "Neighborhoods.txt" }
            };

            TerrainInfo = GetTerrainInfo();
        }

        internal bool EnsureAllFiles()
        {
            foreach (string file in fileNames.Values)
            {
                string path = Path.Combine(WorkingDirectory, file);
                if (!File.Exists(path))
                    return false;
            }
            return true;
        }

        private string BuildPath(string key)
        {
            if (!fileNames.ContainsKey(key))
                throw new ArgumentException($"Key '{key}' not found in file names dictionary.");
            return Path.Combine(WorkingDirectory, fileNames[key]);
        }

        private FileStream getHandleRead(string Key) => new FileStream(BuildPath(Key), FileMode.Open, FileAccess.Read);

        private SC3KTerrainDescription GetTerrainInfo()
        {
            byte[] bmpData = GetElevationChunk(out int w, out int h);

            //build the bmp
            int lowBound = bmpData.Min();
            int highBound = bmpData.Max();

            return new SC3KTerrainDescription(lowBound, highBound);
        }

        private System.Drawing.Color[] getGreyscale()
        {
            System.Drawing.Color[] palette = new System.Drawing.Color[256];
            for (int i = 0; i < 256; i++)
            { // greyscale
                palette[i] = System.Drawing.Color.FromArgb(255, i, i, i);
            }
            return palette;
        }

        private System.Drawing.Color[] getForestColors()
        {
            System.Drawing.Color[] palette = new System.Drawing.Color[256];
            for (int i = 1; i < 12; i++)
            { // greens                
                palette[i] = System.Drawing.Color.FromArgb(255, 0, 255 / i, 0);
            }
            return palette;
        }
        private System.Drawing.Color[] getRedScale()
        {
            System.Drawing.Color[] palette = new System.Drawing.Color[256];
            for (int i = 0; i < 255; i++)
            { // greens                
                palette[i] = System.Drawing.Color.FromArgb(255, i, 0, 0);
            }
            return palette;
        }

        private System.Drawing.Color[] getPalette()
        {
            System.Drawing.Bitmap paletteBMP = new System.Drawing.Bitmap(BuildPath("Palette"));
            System.Drawing.Color[] palette = new System.Drawing.Color[256];

            for (int i = 0; i < 256; i++)
            {
                int x = 0;
                int y = i;
                palette[i] = paletteBMP.GetPixel(x, y);
            }

            paletteBMP.Dispose();
            return palette;
        }

        public byte[] GetElevationChunk(out int W, out int H)
        {
            using FileStream fs = getHandleRead("BIN");

            fs.Seek((long)TSOCityBinFileOffsets.Elevation, SeekOrigin.Begin);

            int readInt()
            {
                byte[] data = new byte[sizeof(uint)];
                fs.ReadExactly(data, 0, data.Length);
                return BitConverter.ToInt32(data);
            }

            byte[] bmp = null;

            int unk = readInt();
            int w = W = readInt();
            int h = H = readInt();

            bmp = new byte[w * h];

            for (int i = 0; i < bmp.Length; i++)
            {
                bmp[i] = (byte)fs.ReadByte();
            }
            return bmp;
        }

        public Bitmap RenderElevationTexture() => renderElevationChunk(getGreyscale(), false);
        public Bitmap RenderTerrainTexture() => renderElevationChunk(getPalette(), false);
        private Bitmap renderElevationChunk(System.Drawing.Color[] Palette, bool HighContrast = false)
        {
            GetElevationChunk(out int w, out int h);

            Color[] terrainColors = GetElevationTerrainColorArray(Palette,HighContrast);
            Bitmap rasterBmp = new System.Drawing.Bitmap(w, h);

            for (int i = 0; i < terrainColors.Length; i++)
            {
                Color colorByte = terrainColors[i];
                int y = i / w;
                int x = i % w;
                rasterBmp.SetPixel(x, y, colorByte);
            }

            return rasterBmp;
        }

        private Color[] GetElevationTerrainColorArray(System.Drawing.Color[] Palette, bool HighContrast = false)
        {
            System.Drawing.Color[] palette = Palette;

            byte[] bmpData = GetElevationChunk(out int w, out int h);
            
            //build the bmp
            int lowBound = bmpData.Min();
            int highBound = bmpData.Max();

            int waterLevel = WaterLevel;
            byte MaxTerrainHeight = 50;
            byte waterTerrainPaletteIndex = 130; // point in palette where it shifts from land +|- sea

            int range = highBound - lowBound;

            Color[] colors = new Color[w*h];

            for (int i = 0; i < bmpData.Length; i++)
            {
                byte colorByte = bmpData[i];
                int y = i / w;
                int x = i % w;

                if (HighContrast)
                    colorByte = (byte)(((colorByte - lowBound) / (double)(highBound - lowBound) * 255));
                else
                { // scale the height to a range of 256, with water at 129, and sand/grass at 128, and snowcap at 0. This is just for fun and not how the game actually renders it, but it makes it easier to visualize the elevation without needing the palette
                    int terrainElevationSeaLevel = colorByte - waterLevel;
                    terrainElevationSeaLevel = Math.Clamp(terrainElevationSeaLevel, -MaxTerrainHeight, MaxTerrainHeight);
                    if (terrainElevationSeaLevel <= 0) // water                    
                        colorByte = (byte)(waterTerrainPaletteIndex + (Math.Abs(terrainElevationSeaLevel) / (double)MaxTerrainHeight) * (byte.MaxValue - waterTerrainPaletteIndex)); // count down from water to black by depth 128 -> 255
                    else
                        colorByte = (byte)Math.Abs(waterTerrainPaletteIndex - ((terrainElevationSeaLevel / (double)MaxTerrainHeight) * (byte.MaxValue - waterTerrainPaletteIndex))); // count up from grass to white snowcap by elevation 127 -> 0              
                }

                System.Drawing.Color renderColor = palette[colorByte];
                colors[i] = renderColor;
            }

            return colors;
        }

        public Bitmap[] RenderForestMiscChunks()
        {
            using FileStream fs = getHandleRead("BIN");

            int bppWidth = 1;
            int w = 256, h = 256;

            //level two
            int images = 2;

            int calcSize = w * h * bppWidth;
            int actualSize = (int)(TSOCityBinFileOffsets.ScreenSpaceUV - TSOCityBinFileOffsets.ForestMisc);

            if (calcSize * images != actualSize)
                throw new InvalidDataException($"Calculated image data size ({calcSize * 2}) does not match actual size ({actualSize})! The file may be malformed or the calculation logic may be incorrect.");

            fs.Seek((long)TSOCityBinFileOffsets.ForestMisc, SeekOrigin.Begin);

            Bitmap[] bmps = new Bitmap[2];

            for (int i = 0; i < images; i++)
            {
                byte[] bmp = new byte[calcSize];

                int readAmt = fs.Read(bmp, 0, bmp.Length);
                if (readAmt != bmp.Length)
                    throw new InvalidDataException($"readamt ({readAmt}) and image expected Size ({bmp.Length}) data length mismatch!");

                nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bmp, 0);
                using MemoryStream ms = new(bmp);
                
                Bitmap renderedBmp = new System.Drawing.Bitmap(w, h, bppWidth * w, PixelFormat.Format8bppIndexed, ptr);
                renderedBmp.Palette = new ColorPalette(getForestColors());
                renderedBmp.MakeTransparent(renderedBmp.Palette.Entries[0]);

                bmps[i] = renderedBmp;
            }

            return bmps;

        }

        public Vector2[] GetScreenSpaceUV(int w, int h)
        {
            using FileStream fs = getHandleRead("BIN");

            Vector2[] uvList = new Vector2[w * h];

            //**read UV map
            long offset = (long)TSOCityBinFileOffsets.ScreenSpaceUV;
            long actualSize = fs.Length - offset;

            fs.Seek(offset, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(fs, Encoding.UTF8, true))
            {
                int index = 0;
                for (int i = 0; i < actualSize / 4; i++)
                {
                    if (fs.Position + 4 > fs.Length) break; // Avoid reading past EOF

                    // Assuming 16-bit Short normalized coordinates based on the file patterns
                    short rawU = reader.ReadInt16();
                    short rawV = reader.ReadInt16();

                    uvList[index] = new Vector2
                    {
                        X = rawU,
                        Y = rawV
                    };
                    index++;
                }
            }

            return uvList;
        }

        public record TSOCityBinNeighborhoodEntry(string Name, int ScrX, int ScrY, int Size);
        public IEnumerable<TSOCityBinNeighborhoodEntry> GetNeighborhoods()
        {
            using StreamReader reader = System.IO.File.OpenText(BuildPath("Neighborhoods"));

            bool started = false;

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                               
                if (line == null) break;
                if (line.Contains("[begin]")) { started = true; continue; }
                if (line.Contains("[end]")) break;
                if (!started) continue;

                //read line
                string[] parts = line.Split(',');
                string name = parts[0];
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
                int size = int.Parse(parts[3].Replace(";", ""));
                // Render the neighborhood on the map using the name, coordinates, and size

                yield return new TSOCityBinNeighborhoodEntry(name,x,y,size);
            }
        }

        public Bitmap RenderScreenSpaceUVTexture(bool Neighborhoods)
        {
            //read el map
            GetElevationChunk(out int w, out int h);

            //get terrain colors
            Color[] elevationTexData = GetElevationTerrainColorArray(getPalette(),false);

            //load deformation
            Vector2[] uvList = GetScreenSpaceUV(w, h);

            float Xlow = uvList.Min(uv => uv.X);
            float Xhigh = uvList.Max(uv => uv.X);
            float Ylow = uvList.Min(uv => uv.Y);
            float Yhigh = uvList.Max(uv => uv.Y);

            int renderW = (int)((Xhigh)) + 50;
            int renderH = (int)((Yhigh)) + 50;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(renderW, renderH);
            {
                using Graphics g = Graphics.FromImage(bmp);

                for (int i = 0; i < uvList.Length; i++)
                {
                    //**read el colorbyte
                    //byte colorIndex = elevation[i]; // Placeholder for actual color byte retrieval logic - done!

                    Vector2 uv = uvList[i];

                    // Normalize UVs to the range of the palette                
                    int y = (int)uv.Y;
                    int x = (int)uv.X;

                    //get color from elevation
                    System.Drawing.Color renderColor = elevationTexData[i];
                    using Brush b = new SolidBrush(renderColor);

                    //render tile 2D
                    int gridU = i % w;
                    int gridV = i / w;

                    float tW = 10, tH = tW / 2f;
                    PointF[] points = [new PointF(x, y), new PointF(x - tW, y), new PointF(x - (tW / 2), y - (tH / 2)), new PointF(x - (tW / 2), y + (tH / 2))];

                    if (false)
                        if (gridU < w - 1 && gridV < w - 1)
                        {
                            int right = i + 1;
                            int below = i + w;
                            int belowRight = i + w + 1;

                            points[1] = new PointF(uvList[right].X, uvList[right].Y);
                            points[2] = new PointF(uvList[below].X, uvList[below].Y);
                            points[3] = new PointF(uvList[belowRight].X, uvList[belowRight].Y);
                        }

                    g.FillPolygon(b, points);
                }
            }

            if (Neighborhoods)
            {
                Color[] palette = getRedScale();

                using System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
                using System.Drawing.Font font = new System.Drawing.Font("Comic Sans MS", 12);

                int color = 256;
                //get neighborhoods
                foreach (TSOCityBinNeighborhoodEntry entry in GetNeighborhoods())
                {
                    color -= 8;

                    using System.Drawing.Brush pen = new System.Drawing.SolidBrush(palette[color]);

                    g.FillEllipse(pen, new System.Drawing.RectangleF(entry.ScrX, entry.ScrY, (entry.Size * 3) + 3, (entry.Size * 3) + 3)); // Example: Mark the neighborhood location with a red dot
                    g.DrawString(entry.Name, font, pen, new System.Drawing.PointF(entry.ScrX + 15, entry.ScrY + 15)); // Example: Mark the neighborhood location with a red dot
                }
            }
            return bmp;
        }

        public TSOSC3KTerrainMesh BuildTerrainMesh(float ElevationScale = 1.0f)
        {
            int GridSize = 256; // Assuming a 256x256 grid based on the file structure

            // Read the deformation data (UVs) from the file
            Vector2[] uvData = GetScreenSpaceUV(GridSize, GridSize);

            byte[] elevationData = GetElevationChunk(out int w, out int h);
            Bitmap texture = RenderTerrainTexture(); // This will read the elevation data and can be used to color the mesh

            // uvData.Length is 65,536 (256 * 256)
            Vector3[] vertices = new Vector3[uvData.Length];
            int[] tris = new int[(GridSize - 1) * (GridSize - 1) * 6];

            int triIndex = 0;

            for (int i = 0; i < uvData.Length; i++)
            {
                // get the grid position from the index
                int gridU = i % GridSize;
                int gridV = i / GridSize;

                // make uv mesh geometry
                float xPos = gridU; //(gridU - gridV); // make isometric
                float zPos = gridV; //(gridU + gridV) * 0.5f;
                float yPos = ElevationScale * elevationData[i]; // Assuming it's flat unless there's a 3rd data stream

                vertices[i] = new Vector3(xPos, yPos, zPos);

                // triangulate the grid (except for the last row and column)
                if (gridU < GridSize - 1 && gridV < GridSize - 1)
                {
                    int root = i;
                    int right = i + 1;
                    int below = i + GridSize;
                    int belowRight = i + GridSize + 1;

                    // Triangle 1
                    tris[triIndex++] = root;
                    tris[triIndex++] = below;
                    tris[triIndex++] = right;

                    // Triangle 2
                    tris[triIndex++] = below;
                    tris[triIndex++] = belowRight;
                    tris[triIndex++] = right;
                }
            }   
            
            return new TSOSC3KTerrainMesh(vertices, tris, uvData, texture);
        }
    }
}

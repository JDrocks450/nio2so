using nio2so.Formats.Img.BMP;
using nio2so.TSOView2.Formats.OBJ;
using nio2so.TSOView2.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
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

namespace nio2so.TSOView2.Formats.CityBIN
{
    /// <summary>
    /// Interaction logic for TSOCityBinPage.xaml
    /// </summary>
    public partial class TSOCityBinPage : Page
    {
        public enum ImportantOffsets : long
        {
            chunk0 = 0x0,
            chunk2 = 0x1020D,
            chunk3 = 0x3020D //B
        }

        public ImportantOffsets CurrentChunk { get; private set; } = ImportantOffsets.chunk0;

        public TSOCityBinPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            doRender();

            ThreeDBox.Checked += ThreeDBox_Checked;
            ThreeDBox.Unchecked += ThreeDBox_Checked;
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
        private System.Drawing.Color[] getPalette()
        {
            System.Drawing.Bitmap paletteBMP = new System.Drawing.Bitmap(@"C:\Program Files (x86)\Maxis\TSO Pre-Alpha\TSO\MatchMaker\CityTerrainPalette.bmp");
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

        private byte[] GetElevationChunk(FileStream FS, out int W, out int H)
        {
            var fs = FS;
            fs.Seek((long)ImportantOffsets.chunk0, SeekOrigin.Begin);

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

        private BitmapImage RenderElevationChunk(FileStream FS)
        {
            FileStream fs = FS;

            //makeshift bmp quick and dirty

            System.Drawing.Color[] palette = getGreyscale();
            palette = getPalette();

            var bmp = GetElevationChunk(fs, out int w, out int h);
            var raster = new System.Drawing.Bitmap(w, h);

            //build the bmp
            int lowBound = bmp.Min();
            int highBound = bmp.Max();
            int range = highBound - lowBound;

            for (int i = 0; i < bmp.Length; i++)
            {
                byte colorByte = bmp[i];
                int y = i / w;
                int x = i % w;

                //colorByte = (byte)((colorByte - lowBound) / (double)(highBound - lowBound) * 255);

                System.Drawing.Color renderColor = palette[colorByte];

                raster.SetPixel(x, y, renderColor);
            }

            return WPFInteropExtensions.Convert(raster);
        }

        private BitmapImage RenderForestChunk(FileStream FS)
        {
            FileStream fs = FS;

            int bppWidth = 2;

            int readInt()
            {
                byte[] data = new byte[sizeof(uint)];
                fs.ReadExactly(data, 0, data.Length);
                return BitConverter.ToInt32(data);
            }

            int w = 256, h = 256;

            //level two
            int calcSize = w * h * bppWidth;
            int actualSize = (int)(ImportantOffsets.chunk3 - ImportantOffsets.chunk2);

            if (calcSize != actualSize)
                ;

            byte[] bmp = new byte[actualSize];
            fs.Seek((long)ImportantOffsets.chunk2, SeekOrigin.Begin);

            int readAmt = fs.Read(bmp, 0, bmp.Length);
            if (readAmt != bmp.Length)
                ;

            nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bmp, 0);
            using MemoryStream ms = new(bmp);
            using var leveltwo = new System.Drawing.Bitmap(w, h, bppWidth * w, System.Drawing.Imaging.PixelFormat.Format16bppRgb565, ptr);

            return WPFInteropExtensions.Convert(leveltwo);

        }

        private Vector2[] GetDeformation(FileStream FS, int w = 256, int h = 256)
        {
            var fs = FS;

            var uvList = new Vector2[w * h];

            //**read UV map
            long offset = (long)ImportantOffsets.chunk3;
            long actualSize = fs.Length - offset;

            fs.Seek(offset, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(fs,Encoding.UTF8,true))
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

        public BitmapImage RenderUVTexture(FileStream FS)
        {
            var fs = FS;

            //read el map
            byte[] elevation = GetElevationChunk(FS,out int w, out int h);

            //load palette
            var palette = getPalette();

            //load deformation
            var uvList = GetDeformation(FS, w, h);

            float Xlow = uvList.Min(uv => uv.X);
            float Xhigh = uvList.Max(uv => uv.X);
            float Ylow = uvList.Min(uv => uv.Y);
            float Yhigh = uvList.Max(uv => uv.Y);

            int renderW = (int)((Xhigh) )+50;
            int renderH = (int)((Yhigh) )+50;

            using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(renderW, renderH);

            for (int i = 0; i < uvList.Length; i++)
            {
                //**read el colorbyte
                byte colorIndex = elevation[i]; // Placeholder for actual color byte retrieval logic

                Vector2 uv = uvList[i];

                // Normalize UVs to the range of the palette
                System.Drawing.Color renderColor = palette[colorIndex];
                int y = (int)uv.Y;
                int x = (int)uv.X;
                bmp.SetPixel(x, y, renderColor);
                
            }            

            return WPFInteropExtensions.Convert(bmp);
        }

        private Model3DCollection RenderTerrainMesh(FileStream FS, float ElevationScale = 1.0f)
        {           
            int GridSize = 256; // Assuming a 256x256 grid based on the file structure

            // Read the deformation data (UVs) from the file
            Vector2[] uvData = GetDeformation(FS,GridSize,GridSize);

            var elevationData = GetElevationChunk(FS, out int w, out int h);
            var texture = RenderElevationChunk(FS); // This will read the elevation data and can be used to color the mesh

            // uvData.Length is 65,536 (256 * 256)
            Vector3[] vertices = new Vector3[uvData.Length];
            int[] tris = new int[(GridSize - 1) * (GridSize - 1) * 6];

            int triIndex = 0;

            for (int i = 0; i < uvData.Length; i++)
            {
                // 1. Get the grid position from the index
                int gridU = i % GridSize;
                int gridV = i / GridSize;

                // 2. Unshear the physical geometry (The Vertex)
                // We rotate the grid indices to create a square 3D footprint
                float xPos = gridU; //(gridU - gridV);
                float zPos = gridV;//(gridU + gridV) * 0.5f;
                float yPos = ElevationScale * elevationData[i]; // Assuming it's flat unless there's a 3rd data stream

                vertices[i] = new Vector3(xPos, yPos, zPos);

                // 2. Triangle Logic (Only run if we aren't on the far right or bottom edge)
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

            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection(vertices.Select(v => new Point3D(v.X, v.Y, v.Z))),
                TriangleIndices = new Int32Collection(tris),
                TextureCoordinates = new PointCollection(uvData.Select(uv => new Point(uv.X / GridSize, uv.Y / GridSize))),                   
            };

            var material = new DiffuseMaterial(new ImageBrush(texture));
            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                BackMaterial = material,
                Transform = new ScaleTransform3D(1, 1, 1)
            };

            return [model];
        }

        private void doRender(ImportantOffsets? Chunk = default)
        {
            try
            {
                string SRC = @"C:\Program Files (x86)\Maxis\TSO Pre-Alpha\TSO\MatchMaker\CityMap.bin";

                if (!Chunk.HasValue)
                    Chunk = CurrentChunk;
                CurrentChunk = Chunk.Value;

                FileStream fs = new FileStream(SRC, FileMode.Open, FileAccess.Read);
                {
                    RenderWindow.Visibility = Visibility.Visible;
                    MeshModelViewer.Visibility = Visibility.Hidden;

                    RenderWindow.Source = CurrentChunk switch
                    {
                        ImportantOffsets.chunk0 => RenderElevationChunk(fs),
                        ImportantOffsets.chunk3 => RenderUVTexture(fs),
                        ImportantOffsets.chunk2 => RenderForestChunk(fs),
                    };

                    if (Chunk == ImportantOffsets.chunk3 && (ThreeDBox.IsChecked ?? false)) // 3D
                    {
                        RenderWindow.Visibility = Visibility.Hidden;
                        MeshModelViewer.Visibility = Visibility.Visible;
                        MeshModelViewer.SetObjects(RenderTerrainMesh(fs,.5f));
                    }
                }
                fs.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rendering chunk: {ex.Message}");
            }
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            doRender();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImportantOffsets index = Enum.GetValues<ImportantOffsets>()[(sender as TabControl).SelectedIndex];
            doRender(index);
        }

        private void ThreeDBox_Checked(object sender, RoutedEventArgs e)
        {
            doRender();
        }
    }
}

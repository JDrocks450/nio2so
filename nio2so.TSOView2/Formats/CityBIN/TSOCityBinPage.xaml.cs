using nio2so.TSOView2.Util;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

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
        private System.Drawing.Color[] getForestColors()
        {
            System.Drawing.Color[] palette = new System.Drawing.Color[256];
            for (int i = 1; i < 10; i++)
            { // greens                
                palette[i] = System.Drawing.Color.FromArgb(255, 0, 255/i, 0);
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
            //palette = palette.Reverse().ToArray();
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

        private BitmapImage RenderElevationChunk(FileStream FS, System.Drawing.Color[] Palette, bool ElevationMode = false)
        {
            FileStream fs = FS;

            //makeshift bmp quick and dirty

            System.Drawing.Color[] palette = Palette;

            var bmp = GetElevationChunk(fs, out int w, out int h);
            var raster = new System.Drawing.Bitmap(w, h);

            //build the bmp
            int lowBound = bmp.Min();
            int highBound = bmp.Max();

            int waterLevel = lowBound;

            int range = highBound - lowBound;

            for (int i = 0; i < bmp.Length; i++)
            {
                byte colorByte = bmp[i];
                int y = i / w;
                int x = i % w;

                if (ElevationMode)
                    colorByte = (byte)(((colorByte - lowBound) / (double)(highBound - lowBound) * 255));
                else
                {
                    colorByte -= (byte)waterLevel;
                    if (colorByte == 0) // water tile
                        colorByte = 138; // make it a nice blue instead of the palette's dark grey
                    else
                    {
                        colorByte = (byte)((colorByte / (double)range) * 128);
                        colorByte = (byte)Math.Abs(128 - colorByte); // count up from grass to white snowcap by elevation (?)
                    }
                }

                System.Drawing.Color renderColor = palette[colorByte];

                raster.SetPixel(x, y, renderColor);
            }

            return WPFInteropExtensions.Convert(raster);
        }

        private BitmapImage[] RenderForestChunks(FileStream FS)
        {
            FileStream fs = FS;

            int bppWidth = 1;
            int w = 256, h = 256;

            //level two
            int images = 2;

            int calcSize = w * h * bppWidth;
            int actualSize = (int)(ImportantOffsets.chunk3 - ImportantOffsets.chunk2);

            if (calcSize * images != actualSize)
                throw new InvalidDataException($"Calculated image data size ({calcSize * 2}) does not match actual size ({actualSize})! The file may be malformed or the calculation logic may be incorrect."); 

            fs.Seek((long)ImportantOffsets.chunk2, SeekOrigin.Begin);

            BitmapImage[] bmps = new BitmapImage[2];

            for (int i = 0; i < images; i++)
            {
                byte[] bmp = new byte[calcSize];
                
                int readAmt = fs.Read(bmp, 0, bmp.Length);
                if (readAmt != bmp.Length)
                    throw new InvalidDataException($"readamt ({readAmt}) and image expected Size ({bmp.Length}) data length mismatch!");

                nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bmp, 0);
                using MemoryStream ms = new(bmp);
                using var renderedBmp = new System.Drawing.Bitmap(w, h, bppWidth * w, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, ptr);

                renderedBmp.Palette = new System.Drawing.Imaging.ColorPalette(getForestColors());

                renderedBmp.MakeTransparent(renderedBmp.Palette.Entries[0]);

                bmps[i] = WPFInteropExtensions.Convert(renderedBmp, true);
            }
            return bmps;

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
            Vector2[] uvData = GetDeformation(FS, GridSize, GridSize);

            var elevationData = GetElevationChunk(FS, out int w, out int h);
            var texture = RenderElevationChunk(FS, getPalette(), false); // This will read the elevation data and can be used to color the mesh

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

            //to wpf mesh
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection(vertices.Select(v => new Point3D(v.X, v.Y, v.Z))),
                TriangleIndices = new Int32Collection(tris),
                TextureCoordinates = new PointCollection(vertices.Select(uv => new Point(uv.X, uv.Z))),
            };
            //make mesh material
            var material = new DiffuseMaterial(new ImageBrush(texture));
            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                //BackMaterial = material,
                Transform = new ScaleTransform3D(1, 1, 1)
            };

            // ** forest layer ** 
            var forestTexture = RenderForestChunks(FS)[0]; // Assuming the first image is the forest layer
            var forestMaterial = new DiffuseMaterial(new ImageBrush(forestTexture));
            var transforms = new Transform3DGroup();
            transforms.Children.Add(new ScaleTransform3D(1, 1, 1));
            transforms.Children.Add(new TranslateTransform3D(0, 0.1, 0)); // Slightly above the terrain to prevent z-fighting
            var forestModel = new GeometryModel3D
            {
                Geometry = mesh,
                Material = forestMaterial,
                //BackMaterial = forestMaterial,
                Transform = transforms
            };

            return [model,forestModel];
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
                    RenderWindow2.Visibility = Visibility.Collapsed;

                    void SetImages()
                    {
                        object VAL = CurrentChunk switch
                        {
                            ImportantOffsets.chunk0 => RenderElevationChunk(fs, getGreyscale(), true),
                            ImportantOffsets.chunk3 => RenderUVTexture(fs),
                            ImportantOffsets.chunk2 => RenderForestChunks(fs),
                        };
                        if (VAL is BitmapImage[] arr)
                        { // lol hackish solution
                            RenderWindow2.Visibility = Visibility.Visible;
                            RenderWindow2.Source = arr[1];
                            VAL = arr[0];
                        }
                        RenderWindow.Source = VAL as ImageSource;
                    }
                    SetImages();

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

        private void CopyButton_Click(object sender, RoutedEventArgs e) => Clipboard.SetImage(RenderWindow.Source as BitmapSource);
    }
}

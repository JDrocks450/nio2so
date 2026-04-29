using Microsoft.Win32;
using nio2so.Formats.Terrain.SC3K;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private bool Ready = false;
        private SC3KTerrainImporter Importer;
        public SC3KTerrainImporter.TSOCityBinFileOffsets CurrentChunk { get; private set; } = default;

        public enum OpenMode
        {
            AutomaticTSODirectory,
            ManualDirectorySelect
        }
        private readonly OpenMode openMode = OpenMode.AutomaticTSODirectory;

        public TSOCityBinPage()
        {
            InitializeComponent();
        }
        public TSOCityBinPage(OpenMode OpenMode) : this()
        {
            openMode = OpenMode;
        }

        private string? PromptSelectDirectory()
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog
            {
                Title = "Select your The Sims Online MatchMaker Directory",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Multiselect = false,
                DereferenceLinks = true,
                ValidateNames = true,
            };
            if (folderDialog.ShowDialog() ?? false)
            {
                return folderDialog.FolderName;
            }
            return null;
        }

        private bool SetupDirectoryStructures()
        {
            //tso directory set?
            string? tsoDir = default;

            if (openMode == OpenMode.AutomaticTSODirectory)
            {
                tsoDir = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;

                // check it, if null, prompt user to set it in settings
                if (tsoDir == default)
                    TSOViewConfigHandler.EnsureSetGameDirectoryFirstRun();

                // check it now.
                tsoDir = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;

                if (tsoDir != default) // its set! navigate to matchmaker dir
                    tsoDir = System.IO.Path.Combine(tsoDir, "matchmaker");
                //if not set, handle it below with ManualSelectionLogic
            }

            if (tsoDir == default) // still missing.            
                tsoDir = PromptSelectDirectory();   //tso dir will now be matchmaker dir
            
            if (tsoDir == default)           
                return false; // literally still missing -- user cancelled. silent return             

            try
            {
                Importer = new SC3KTerrainImporter(new DirectoryInfo(tsoDir));

                WaterSlider.Maximum = Importer.TerrainInfo.TerrainHighestElevation;
                WaterSlider.Minimum = Importer.TerrainInfo.TerrainLowestElevation;
                WaterSlider.Value = Importer.TerrainInfo.WaterLevel;                
            }
            catch (FileNotFoundException ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //**start your engines**
            //rev up the importer
            if (!SetupDirectoryStructures())
                return;

            //** clear to start rendering processes

            Ready = true;
            doRender();

            ThreeDBox.Checked += ThreeDBox_Checked;
            ThreeDBox.Unchecked += ThreeDBox_Checked;
        }

        private Model3DCollection RenderTerrainMesh(float ElevationScale = 1.0f)
        {
            //get the mesh data
            using TSOSC3KTerrainMesh sc3kMesh = Importer.BuildTerrainMesh(ElevationScale);

            //to wpf mesh
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = [.. sc3kMesh.Vertices.Select(v => new Point3D(v.X, v.Y, v.Z))],
                TriangleIndices = [.. sc3kMesh.Indices],
                TextureCoordinates = [.. sc3kMesh.Vertices.Select(uv => new System.Windows.Point(uv.X, uv.Z))],
            };
            //make mesh material
            DiffuseMaterial material = new DiffuseMaterial(new ImageBrush(WPFInteropExtensions.Convert(sc3kMesh.TerrainTexture)));
            GeometryModel3D model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                //BackMaterial = material,
                Transform = new ScaleTransform3D(1, 1, 1)
            };

            // ** forest layer ** 

            //get forest texture
            using System.Drawing.Bitmap forestTexture = Importer.RenderForestMiscTextures()[0]; // Assuming the first image is the forest layer

            //material
            DiffuseMaterial forestMaterial = new DiffuseMaterial(new ImageBrush(WPFInteropExtensions.Convert(forestTexture,true)));
            Transform3DGroup transforms = new Transform3DGroup();
            transforms.Children.Add(new ScaleTransform3D(1, 1, 1));
            transforms.Children.Add(new TranslateTransform3D(0, 0.1, 0)); // Slightly above the terrain to prevent z-fighting
            GeometryModel3D forestModel = new GeometryModel3D
            {
                Geometry = mesh,
                Material = forestMaterial,
                //BackMaterial = forestMaterial,
                Transform = transforms
            };

            return [model, forestModel];
        }

        private void doRender(SC3KTerrainImporter.TSOCityBinFileOffsets? Chunk = default)
        {
            if (!Ready) return;
            try
            {
                if (!Chunk.HasValue)
                    Chunk = CurrentChunk;
                CurrentChunk = Chunk.Value;

                RenderWindow.Visibility = Visibility.Visible;
                MeshModelViewer.Visibility = Visibility.Hidden;
                RenderWindow2.Visibility = Visibility.Collapsed;

                Importer.TerrainInfo.WaterLevel = (int)WaterSlider.Value;

                void SetImages()
                {
                    object VAL = CurrentChunk switch
                    {
                        SC3KTerrainImporter.TSOCityBinFileOffsets.Elevation => new System.Drawing.Bitmap[] {
                                Importer.RenderElevationTexture(),
                                Importer.RenderTerrainTexture()
                            },
                        SC3KTerrainImporter.TSOCityBinFileOffsets.ScreenSpaceUV => Importer.RenderScreenSpaceUVTexture(),
                        SC3KTerrainImporter.TSOCityBinFileOffsets.ForestMisc => Importer.RenderForestMiscTextures(),
                    };
                    if (VAL is System.Drawing.Bitmap[] bmpArr)
                    {
                        List<BitmapImage> images = new List<BitmapImage>();
                        foreach (var bmp in bmpArr) {
                            images.Add(WPFInteropExtensions.Convert(bmp));
                            bmp.Dispose();
                        }
                        VAL = images.ToArray();
                    }
                    if (VAL is BitmapImage[] arr)
                    { // lol hackish solution
                        RenderWindow2.Visibility = Visibility.Visible;
                        RenderWindow2.Source = arr[1];
                        VAL = arr[0];
                    }
                    if (VAL is System.Drawing.Bitmap drawingBMP)
                    {
                        VAL = drawingBMP.Convert();
                        drawingBMP.Dispose();
                    }
                    RenderWindow.Source = VAL as ImageSource;
                }
                SetImages();

                if (Chunk == SC3KTerrainImporter.TSOCityBinFileOffsets.ScreenSpaceUV && (ThreeDBox.IsChecked ?? false)) // 3D
                { // render 3D terrain mesh
                    RenderWindow.Visibility = Visibility.Hidden;
                    MeshModelViewer.Visibility = Visibility.Visible;
                    MeshModelViewer.SetObjects(RenderTerrainMesh(.5f));
                }
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
            if (!Ready) return;
            SC3KTerrainImporter.TSOCityBinFileOffsets index = Enum.GetValues<SC3KTerrainImporter.TSOCityBinFileOffsets>()[(sender as TabControl).SelectedIndex];
            doRender(index);
        }

        private void ThreeDBox_Checked(object sender, RoutedEventArgs e)
        {
            doRender();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var img = ((e.Source as MenuItem).Parent as ContextMenu).PlacementTarget as Image;
            if (img == null) return;
            Clipboard.SetImage(img.Source as BitmapSource);
        }
    }
}

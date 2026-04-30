using Microsoft.Win32;
using nio2so.TSOView2.FileDialog;
using nio2so.TSOView2.Formats;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows;

namespace nio2so.TSOView2.Plugins
{
    /// <summary>
    /// This plugin will take a The Sims Online Release / New & Improved city and make it viewable in
    /// The Sims Online Pre-Alpha
    /// </summary>
    internal class TSOCityTransmogrifier : TSOViewPluginBase
    {
        /// <summary>
        /// Change all Grass tiles to be Snow terrain tiles
        /// </summary>
        public static Boolean LetItSnow { get; set; } = false;

        public override string PluginName => "Convert to Pre-Alpha City (City Transmogrifier)";

        public override string PluginDescription => WELCOMEWAGON;

        const string troll = "Are you resizing a Pre-Alpha map for use in Pre-Alpha? If so, my job is done already!";

        const string WELCOMEWAGON = "Welcome to the TSO: Pre-Alpha city Transmogrifier!\r\n" +
            "First you will select a city from The Sims Online Release 1.0 or onward, then this tool " +
            "will make city files compatible with The Sims Online Pre-Alpha which you can then replace with " +
            "the original GameData/FarZoom folder.";

        public override void Do(ITSOView2Window Parent)
        {
            RunPlugin();
        }

        public static void RunPlugin(string? CityFolder = null, string? ExportFolder = default)
        {
            if (!TSOViewConfigHandler.EnsureSetGameDirectoryFirstRun()) return;
            
            string sourceDirectory = null;
            string outputDirectory = ExportFolder ?? Path.Combine(Environment.CurrentDirectory, "cityoutput");

            bool resizeSucceeded = false;
            
            if (CityFolder != null)
            {
                MessageBoxResult result = MessageBox.Show($"Your city is stored here:\n\n{CityFolder}\n\nIs that right?", 
                    "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes) sourceDirectory = CityFolder;
            }            
            if (sourceDirectory == null)
            {
                MessageBox.Show(WELCOMEWAGON);

                //PROMPT FOR CITY DIRECTORY
                if (!FileDialogHandler.ShowUser_SelectCityFolder(out string? FileName) ?? true)
                    return; // user dismissed the dialog away
                sourceDirectory = FileName;
            }
            if (string.IsNullOrEmpty(sourceDirectory)) return;

            //make sure the directory is valid
            Exception obj = null;
            try
            {
                checkDirectory(sourceDirectory);
            }
            catch (Exception ex)
            {
                obj = ex;
                goto error;
            }

            //all files present -- choose output folder            
            OpenFolderDialog dlg = new OpenFolderDialog()
            {
                Title = "Choose Export Folder",
                InitialDirectory = outputDirectory,
                Multiselect = false,
            };
            if (!dlg.ShowDialog() ?? true)
                return;
            outputDirectory = dlg.FolderName;

            try
            {
                ResizeAndSave(sourceDirectory, outputDirectory);
            }
            catch (Exception ex)
            {
                obj = ex;
                goto error;
            }

            //**TRANSPORT NOW
            resizeSucceeded = true;
            string destination = Path.Combine(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_GameDataDirectory, "FarZoom");
            if (MessageBox.Show($"Your city has been converted.\n\n{destination}\n\nReady to apply it to your The Sims Online: Pre-Alpha game?",
                "Add city to The Sims Online: Pre-Alpha?", MessageBoxButton.YesNo)
                != MessageBoxResult.Yes)
            {
                Process.Start("explorer", outputDirectory);
                return;
            }
            
            try
            {
                doAddToGame(outputDirectory, destination);
            }
            catch (Exception ex)
            {
                obj = ex;                
                goto error;
            }
        error:
            if (obj == null) return;
            MessageBox.Show(obj.Message);
            if (resizeSucceeded)
                Process.Start("explorer", outputDirectory);
        }

        private static void doAddToGame(string OutputFiles, string Destination)
        {
            string destination = Destination;
            string output = OutputFiles;
            if (!Directory.Exists(destination)) throw new DirectoryNotFoundException(destination);
            string backupDir = destination + "_backup";
            if (!Directory.Exists(backupDir))
            { // no backup yet, make one now
                recurscpy(destination, backupDir);
                if (!Directory.Exists(backupDir)) throw new DirectoryNotFoundException(backupDir);
            }
            foreach (string file in Directory.GetFiles(output))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);

        }

        private static void recurscpy(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        /// <summary>
        /// Checks the given directory for core files for a city and will throw
        /// an exception when one is missing to prevent execution
        /// </summary>
        /// <param name="Directory"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private static bool checkDirectory(string Directory)
        {
            string makedirstring(string fname) => Path.Combine(Directory, fname);
            string currentFile = "elevation.bmp";
            if (!File.Exists(makedirstring(currentFile))) goto onError;
            currentFile = "terraintype.bmp";
            if (!File.Exists(makedirstring(currentFile))) goto onError;
            currentFile = "vertexcolor.bmp";
            if (!File.Exists(makedirstring(currentFile))) goto onError;
            return true;
        onError:
            throw new FileNotFoundException($"The file {currentFile} was not found in the directory:\n\n{Directory}\n\n{troll}");
        }
        /// <summary>
        /// All files in the city in Release and onward are 512x512... pre-alpha is only 256x256.
        /// We have to fix this. This function will resize all bmps to 256x256 and save them to the 
        /// output directory
        /// </summary>
        private static void ResizeAndSave(string SourceDirectory, string DestinationDirectory)
        {
            string makesoucdirstring(string fname) => Path.Combine(SourceDirectory, fname);
            Directory.CreateDirectory(DestinationDirectory);

            string[] fileList = Directory.GetFiles(SourceDirectory, "*.bmp");
            if (!fileList.Any()) throw new FileNotFoundException("No bmps in " + SourceDirectory);

            foreach (string bmpFile in fileList)
            {
                Bitmap bmp = new Bitmap(bmpFile);
                PixelFormat format = bmp.PixelFormat;
                bmp.Dispose();
                bool? success = null;
                if (format == PixelFormat.Format8bppIndexed)
                    success = handle8bppindexedresize(bmpFile, DestinationDirectory);
                else if (format == PixelFormat.Format24bppRgb)
                    handleregresize(bmpFile, DestinationDirectory);
                else throw new InvalidDataException(Path.GetFileName(bmpFile) + " is: " + format.ToString() + " and cannot be resized. ");
                
                if (success.HasValue && !success.Value)
                    throw new Exception($"Cannot resize this city. File: {bmpFile}" + troll);
            }
        }

        private static bool handle8bppindexedresize(string bmpFile, string DestinationDirectory)
        {
            bool success = false;

            string makedestdirstring(string fname) => Path.Combine(DestinationDirectory, fname);

            ColorPalette colorPalette = null;
            using (Bitmap bmp = (Bitmap)Image.FromFile(bmpFile))
                colorPalette = bmp.Palette;            
            
            //load the city we want to transfer to Pre-Alpha
            Bitmap8bpp releaseTSOBmp = new Bitmap8bpp(bmpFile);
            //check the size
            if (releaseTSOBmp.Width != 512 || releaseTSOBmp.Height != 512)            
                goto imdone;            
            //make a new 8bppindexed bmp -- has to be 256 color palette indexed bmp to work ugh
            Bitmap8bpp compatBmp = new Bitmap8bpp(256, 256, colorPalette);            
            //make the name what it would be in TSO pre alpha
            string mynewname = Path.GetFileNameWithoutExtension(bmpFile) + "00.bmp";

            byte? getColorIndex(System.Drawing.Color Color)
            {
                Color[] palette = colorPalette.Entries;
                for (int i = 0; i < palette.Length; i++)
                {
                    Color current = palette[i];
                    if (current.R == Color.R && Color.G == current.G && Color.B == current.B)
                        return (byte)i;
                }
                return default;
            }

            //**HOLIDAY TWEAK**
            byte? snowIndex = getColorIndex(System.Drawing.Color.White);
            byte? grassIndex = getColorIndex(Color.FromArgb(255,0,255,0));
            //**

            for (int x = 0; x < compatBmp.Width; x++)
            {
                for (int y = 0; y < compatBmp.Height; y++)
                {
                    byte color = releaseTSOBmp.GetPixel(x * 2, y * 2);
                    if (LetItSnow)
                    { // Snow holiday easter-egg
                        if (grassIndex.HasValue && snowIndex.HasValue && color == grassIndex)
                            color = snowIndex.Value;
                    }
                    compatBmp.SetPixel(x, y, color);
                }
            }
            using (MemoryStream ms = new MemoryStream())
            {
                compatBmp.Save(makedestdirstring(mynewname));
                mynewname = Path.GetFileNameWithoutExtension(bmpFile) + "00thumb.bmp";
                compatBmp.Save(makedestdirstring(mynewname));
            }
            compatBmp.Dispose();
            success = true;
        imdone:
            releaseTSOBmp.Dispose();
            return success;
        }
        private static bool handleregresize(string bmpFile, string DestinationDirectory)
        {
            bool success = false;

            string makedestdirstring(string fname) => Path.Combine(DestinationDirectory, fname);
            //load the city we want to transfer to Pre-Alpha
            Bitmap releaseTSOBmp = new Bitmap(bmpFile);
            //check the size
            if (releaseTSOBmp.Width != 512 || releaseTSOBmp.Height != 512) goto imdone;
            //make a new 8bppindexed bmp -- has to be 256 color palette indexed bmp to work ugh
            Bitmap compatBmp = new Bitmap(256, 256, releaseTSOBmp.PixelFormat);
            //make the name what it would be in TSO pre alpha
            string mynewname = Path.GetFileNameWithoutExtension(bmpFile) + "00.bmp";

            for (int x = 0; x < compatBmp.Width; x++)
            {
                for (int y = 0; y < compatBmp.Height; y++)
                {
                    Color color = releaseTSOBmp.GetPixel(x * 2, y * 2);
                    compatBmp.SetPixel(x, y, color);
                }
            }
            using (MemoryStream ms = new MemoryStream())
            {
                compatBmp.Save(makedestdirstring(mynewname), ImageFormat.Bmp);
                mynewname = Path.GetFileNameWithoutExtension(bmpFile) + "00thumb.bmp";
                compatBmp.Save(makedestdirstring(mynewname), ImageFormat.Bmp);
            }
            compatBmp.Dispose();
            success = true;
        imdone:
            releaseTSOBmp.Dispose();
            return success;
        }
    }
}

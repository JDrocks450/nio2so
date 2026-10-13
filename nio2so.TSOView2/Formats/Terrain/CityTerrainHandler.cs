using nio2so.Formats.Terrain;
using nio2so.TSOView2.FileDialog;
using nio2so.TSOView2.Formats.OBJ;
using nio2so.TSOView2.Util;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace nio2so.TSOView2.Formats.Terrain
{
    internal class CityTerrainHandler
    {
        public static CityTerrainHandler Current { get; private set; }
        /// <summary>
        /// The currently opened <see cref="TSOCity"/> in the User Interface
        /// </summary>
        public TSOCity City { get; }
        /// <summary>
        /// The currently opened <see cref="TSOCityMesh"/> in the User Interface
        /// </summary>
        public TSOCityMesh Mesh { get; }
        public TSOCityContentManager ContentManager => TSOCityImporter.GlobalDefaultContent;

        /// <summary>
        /// Creates a new instance of the <see cref="CityTerrainHandler"/> and overwrites the previous <see cref="CityTerrainHandler.Current"/> value
        /// </summary>
        /// <param name="city"></param>
        /// <param name="mesh"></param>
        private CityTerrainHandler(TSOCity city, TSOCityMesh mesh)
        {
            Current = this;
            City = city;
            Mesh = mesh;
        }

        public static async Task<CityTerrainHandler?> PromptLoadCity(TSOVersion Version, Func<TSOView2LoadingStatus, Task> OnStatusUpdateCallback)
        {
            //IS THE DIRECTORY SELECTED? 
            if (!TSOViewConfigHandler.EnsureSetGameDirectoryFirstRun()) return null; // idk anymore

            //TSO CONFIG FILE
            string? gamePath = TSOViewConfigHandler.CurrentConfiguration.GetDirectoryByVersion(Version);
            if (gamePath == default) throw new NullReferenceException("Game directory not set.");

            //PROMPT FOR CITY DIRECTORY
            if (!FileDialogHandler.ShowUser_SelectCityFolder(out string FileName) ?? true)
                return null; // user dismissed the dialog away
                        
            return await LoadCity(FileName, Version, OnStatusUpdateCallback);
        }        

        public static async Task<CityTerrainHandler?> LoadCity(string FolderName, TSOVersion Version, Func<TSOView2LoadingStatus, Task> OnStatusUpdateCallback)
        {
            //TSO CONFIG FILE
            string? gamePath = TSOViewConfigHandler.CurrentConfiguration.GetDirectoryByVersion(Version);
            if (gamePath == default) throw new NullReferenceException("Game directory is not selected.");

            //note: tasks should be 1 more than the tasks in this function, as there is one more task to run after this function
            int TASKS = 6, currentTaskIndex = 0;
            TSOView2LoadingStatus? currentStatus = null;

            //run task fire and forget -- ui thread will need to use the dispatcher to maintain thread safety
            async Task StatusUpdate(string verb, bool completed = false, double taskProgress = .1)
            {
                currentStatus = new TSOView2LoadingStatus("Loading City", verb, ++currentTaskIndex / (double)TASKS, completed, taskProgress);
                if (OnStatusUpdateCallback != null) // fire and forget invoke()
                    await OnStatusUpdateCallback?.Invoke(currentStatus.Value);
            }

            try
            {
                //CREATE CITY LOADER
                await StatusUpdate("parsing city terrain files");                
                TSOPreAlphaCityImporter importer = new(gamePath, FolderName)
                {
                    CitySettings = new()
                    {
                        ElevationScale = 1 / 12.0
                    }
                };

                //LOAD ASSETS
                await StatusUpdate("loading texture assets");
                await importer.LoadAssetsAsync(Version == TSOVersion.PreAlpha); // asset loader

                //LOAD GEOMETRY TO MEM
                await StatusUpdate("generating mesh geometry");
                (TSOCity city, TSOCityMesh mesh) values = await importer.LoadCityAsync(); // parse assets into city render

                //NORMALS
                await StatusUpdate("calculating normals");
                //await values.mesh.CalculateNormals(values.city); // mandatory                                  

                //LOAD PLUG-IN
                await StatusUpdate($"creating the {nameof(CityTerrainHandler)}");                
                return new CityTerrainHandler(values.city, values.mesh);
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occured while {currentStatus?.CurrentTask ?? "Catastrophic Error"} for the city ({System.IO.Path.GetFileName(FolderName)}): \n{e.Message}");
            }
            return default; // default fail-out
        }

        public static async void PromptUserShowCityPlugin(Func<TSOView2LoadingStatus, Task> OnStatusUpdateCallback) => 
            await PromptUserShowCityPlugin(TSOVersion.PreAlpha, OnStatusUpdateCallback);

        public static async Task PromptUserShowCityPlugin(TSOVersion Version, Func<TSOView2LoadingStatus, Task> OnStatusUpdateCallback)
        {
            var handler = await PromptLoadCity(Version, OnStatusUpdateCallback);
            if (handler == null) return; // process above failure

            TSOCityViewPage page = new();
            page.Mini_HUD_Image.Source = Current.Mesh.VertexColorAtlas.Convert();

            await OnStatusUpdateCallback(new TSOView2LoadingStatus("Rendering City", "Entering the City", 1.0, false));

            (Application.Current.MainWindow as ITSOView2Window).ShowPlugin(page);

            await OnStatusUpdateCallback(new TSOView2LoadingStatus("Rendering City", "Entering the City", 1.0, true)); // hide the loading window
        }
    }
}

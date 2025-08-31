using nio2so.Formats.Img.Targa;
using nio2so.Formats.UI.UIScript;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using static nio2so.Formats.UI.TSOTheme.TSOThemeFile;

namespace nio2so.Formats.UI.TSOTheme
{
    [Serializable]
    public class TSOThemeDefinition : IDisposable
    {
        public TSOThemeDefinition(string? filePath)
        {
            FilePath = filePath;
        }
        public string? FilePath { get; set; }

        /// <summary>
        /// Use the <see cref="TSOThemeFile.Initialize(string, UIScriptFile, out string[])"/> function to populate this property.
        /// </summary>
        [JsonIgnore]
        [TSOUIScriptEditorVisible(false)]
        public Image? TextureRef { get; internal set; }

        /// <summary>
        /// Lists the names of <see cref="UIScriptObject"/>s that reference this image.
        /// <para/>Use the <see cref="TSOThemeFile.Initialize(string, UIScriptFile, out string[])"/> function to populate this property.
        /// </summary>
        [TSOUIScriptEditorVisible(false)]
        public IEnumerable<string> ReferencedBy { get; set; } = new List<string>();

        public void Dispose()
        {
            TextureRef?.Dispose();
            TextureRef = null;  
        }
    }

    public class TSOThemeHeaderInformationDefinition : TSOThemeDefinition
    {
        /// <summary>
        /// The indices of header information in the definition
        /// </summary>
        private enum ThemeHeaderPositions
        {
            /// <summary>
            /// The first index of the <see cref="ReferencedBy"/> list
            /// </summary>
            VERSION = 0,
        }

        public static TSOThemeHeaderInformationDefinition FromDefinition(TSOThemeDefinition Definition)
        {
            return new TSOThemeHeaderInformationDefinition(Definition.FilePath)
            {
                ReferencedBy = Definition.ReferencedBy,
            };
        }

        public TSOThemeHeaderInformationDefinition(string? filePath) : base(filePath)
        {
            
        }

        public ThemeVersionNames VersionName
        {
            get
            {
                if (!Enum.TryParse<ThemeVersionNames>(SafeAccess((int)ThemeHeaderPositions.VERSION), true, out var result))
                    result = ThemeVersionNames.NotSet;
                return result;
            }
            set => Ensure((int)ThemeHeaderPositions.VERSION)[(int)ThemeHeaderPositions.VERSION] = value.ToString();
        }

        private string SafeAccess(int Index)
        {
            var list = ((List<string>)ReferencedBy);
            while (list.Count < Index+1) list.Add("NULL");
            return list[Index];
        }

        private List<string> Ensure(int Index)
        {
            var list = ((List<string>)ReferencedBy);
            while (list.Count <= Index) list.Add("NULL");
            return list;
        }
    }

    /// <summary>
    /// A Map of AssetIDs (See: <see cref="UIScript.UIScriptDefineComponent"/>) [assetid Property] to a definition providing context to display it correctly.
    /// <para>This context can consist to a filepath on the disk to find the image, etc.</para>
    /// </summary>
    public class TSOThemeFile : Dictionary<ulong, TSOThemeDefinition>, ITSOImportable
    {       
        /// <summary>
        /// Creates a new Theme File. Please use: <see cref="TSOThemeFile.TSOThemeFile(ThemeVersionNames)"/> for versioning
        /// </summary>
        [JsonConstructor] public TSOThemeFile()
        {
            
        }
        /// <summary>
        /// Creates a new Theme File with a version encoded
        /// </summary>
        public TSOThemeFile(ThemeVersionNames Version) : this()
        {
            Add(0, new TSOThemeHeaderInformationDefinition(null));
            SetVersionName(Version);
        }

        public enum ThemeVersionNames
        {
            NotSet,
            PreAlpha,
            NandI
        }

        /// <summary>
        /// The version of The Sims Online this theme is for
        /// </summary>
        public ThemeVersionNames GetVersionName() {
            if (!this.ContainsKey(0)) return ThemeVersionNames.NotSet;
            return TSOThemeHeaderInformationDefinition.FromDefinition(this[0]).VersionName;
        }
        public void SetVersionName(ThemeVersionNames value) 
        {
            var definition = TSOThemeHeaderInformationDefinition.FromDefinition(this[0]);
            definition.VersionName = value;
            this[0] = definition;
        }

        [Obsolete] public int TryMrsShipper(UIScriptFile File)
        {
            MrsShipper.DereferenceImageDefines(File, this, out int completed);
            return completed;
        }
        /// <summary>
        /// Uses the packingslips.log file to update the current theme's database of asset IDs.
        /// </summary>
        /// <param name="TSODirectory"></param>
        public void UpdateDatabaseWithMrsShipper(string TSODirectory) => MrsShipper.BreakdownPackingslips(TSODirectory, this);

        public bool Initialize(string BaseDirectory, UIScriptFile Script, out string[] MissingItems)
        {
            Free();
            bool success = LoadImages(BaseDirectory, Script, out MissingItems);
            if (!success) return false;
            MapControlsToImages(Script);
            return true;
        }

        private void MapControlsToImages(UIScriptFile Script)
        {
            foreach(var control in Script.Controls)
            {
                var imgProperty = control.GetProperty("image");
                if (imgProperty == default) continue;
                var imageName = imgProperty.GetValue<UIScriptString>();
                var define = Script.GetDefineByName(imageName);
                if (define == null) continue;
                ((List<String>)this[define.GetAssetID()].ReferencedBy).Add(control.Name);
            }
        }

        private void Free()
        {
            foreach (var img in Values.Where(x => x.TextureRef != null))
                img.Dispose();
        }

        /// <summary>
        /// Loads all defined images into the <see cref="TSOThemeDefinition"/>s added to this object.
        /// <para>Note: Prior to calling this, you should ensure all <see cref="TSOThemeDefinition.FilePath"/> 
        /// are accurate and relative to the <paramref name="BaseDirectory"/></para>
        /// </summary>
        /// <param name="BaseDirectory"></param>
        public bool LoadImages(string BaseDirectory, UIScriptFile Script, out string[] MissingItems)
        {
            List<string> missings = new();
            bool completelySuccessful = true;
            foreach(var define in Script.Defines)
            {
                if (define.Type.ToLower() != "image") // Images
                    continue;
                TSOThemeDefinition definition = default;
                try
                {
                    if (!define.TryGetReference(this, out definition, out ulong assetID))
                    {
                        completelySuccessful = false;
                        missings.Add(define.Name);  
                        continue;
                    }
                }
                catch(InvalidDataException e)
                {
                    completelySuccessful = false;
                    missings.Add(define.Name);
                    continue;
                }
                if (definition?.FilePath == null) continue;
                string commentFilePath = definition.FilePath;
                bool trimmed = false;
                while (commentFilePath.Any())
                {
                    if (!char.IsLetterOrDigit(commentFilePath[0]))
                    {
                        commentFilePath = commentFilePath.Substring(1);
                        continue;
                    }
                    trimmed = true;
                    break;
                }
                if (!trimmed) continue;
                string path = Path.Combine(BaseDirectory, commentFilePath);
                if (!TrySafeLoadTexture(path, out Image? Reference) || Reference == null)
                {
                    completelySuccessful = false;
                    missings.Add(define.Name);
                    continue;
                }
                if (definition.TextureRef != null)
                    definition.Dispose();
                definition.TextureRef = Reference;
            }
            MissingItems = missings.ToArray();
            return completelySuccessful;
        }

        private bool TrySafeLoadTexture(string PathToImage, out Image? Reference)
        {
            Reference = null;
            string path = PathToImage;

            if (!File.Exists(path))
            { // file not found! try looking in a FAR3 archive nearby?
                if (!TryExtractFromFAR3(path, out byte[] dataArray))
                    return false;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, dataArray); // maybe delete this later? ehhhh
            }            
            Image bmp = default;
            if (path.EndsWith(".bmp"))
                bmp = Image.FromFile(path);
            else if (path.EndsWith(".tga"))
                bmp = TargaImage.LoadTargaImage(path);
            Reference = bmp;
            return Reference != null;
        }

        /// <summary>
        /// Extracts the given resource from a FAR file at the location on the disk provided
        /// <para>You do not need to point to the *.dat (FAR) archive itself, a resource URI for what is located inside the FAR
        /// archive is acceptable, provided it is the only FAR archive in the directory you provide.</para>
        /// </summary>
        /// <param name="DesiredResourceURIPath"></param>
        private bool TryExtractFromFAR3(string DesiredResourceURIPath, out byte[] Data)
        {
            Data = default;
            try
            {
                DesiredResourceURIPath = Path.GetFullPath(DesiredResourceURIPath);
                string tempPath = Path.GetDirectoryName(DesiredResourceURIPath);
                while(tempPath.Length > 3)
                { // we need to unwrap this path to find the point where it switches from the disk to a FAR3 archive
                    if (!Directory.Exists(tempPath))
                        tempPath = Path.GetDirectoryName(tempPath);
                    else
                    {
                        IEnumerable<string> tempfiles = Directory.EnumerateFiles(tempPath, "*.dat");
                        if (!tempfiles.Any()) tempPath = Path.GetDirectoryName(tempPath);
                        else break;
                    }
                }                
                IEnumerable<string> files = Directory.EnumerateFiles(tempPath, "*.dat");
                if (!files.Any()) return false;

                string ArchiveURI = DesiredResourceURIPath.Substring(tempPath.Length).TrimStart('\\');
                var FARArchive = files.First();
                using FAR3.FAR3Archive archive = new FAR3.FAR3Archive(FARArchive);
                Data = archive[Path.GetFileName(DesiredResourceURIPath)];
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <summary>
        /// Saves to disk wherever you indicate by <paramref name="FilePath"/>
        /// </summary>
        /// <param name="FilePath"></param>
        public void Save(string FilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath,
                JsonSerializer.Serialize<TSOThemeFile>(this, new JsonSerializerOptions()
            {
                WriteIndented = true,
            }));
        }     
        
        /// <summary>
        /// WARNING: Do not use <see cref="Dictionary{TKey, TValue}.Clear()"/> method. Only use this one
        /// because the versioning will be stripped and cause exceptions.
        /// <para>Not to mention cause memory leaks if already initialized.</para>
        /// </summary>
        public new void Clear()
        {
            Free(); // free memory
            var versioning = this[0] as TSOThemeHeaderInformationDefinition;
            base.Clear();
            Add(0, versioning);
        }
    }

    public class TSOThemeFileImporter : TSOFileImporterBase<TSOThemeFile>
    {
        public static TSOThemeFile Import(string FilePath) => LogConsoleBeforeLeaving(new TSOThemeFileImporter().ImportFromFile(FilePath));
        public override TSOThemeFile Import(Stream stream) => LogConsoleBeforeLeaving(JsonSerializer.Deserialize<TSOThemeFile>(stream));
        private static TSOThemeFile LogConsoleBeforeLeaving(TSOThemeFile File)
        {
            if (File == null) return null;
            Debug.WriteLine($"[TSOThemes] NOTICE! ThemeFile opened from disk. Version: {File.GetVersionName()}");
            return File;
        }
    }
}

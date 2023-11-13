using nio2so.Formats.Img.Targa;
using nio2so.Formats.UI.UIScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        [JsonIgnore]
        [TSOUIScriptEditorVisible(false)]
        public IEnumerable<string> ReferencedBy { get; internal set; } = new List<string>();

        public void Dispose()
        {
            TextureRef?.Dispose();
            TextureRef = null;  
        }
    }

    /// <summary>
    /// A Map of AssetIDs (See: <see cref="UIScript.UIScriptDefineComponent"/>) [assetid Property] to a definition providing context to display it correctly.
    /// <para>This context can consist to a filepath on the disk to find the image, etc.</para>
    /// </summary>
    public class TSOThemeFile : Dictionary<ulong, TSOThemeDefinition>, ITSOImportable
    {
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
            UnloadPreviousSession();
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

        private void UnloadPreviousSession()
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
                if (definition == null) continue;
                if (definition.FilePath.StartsWith('/') || definition.FilePath.StartsWith('\\'))
                    definition.FilePath = definition.FilePath.Substring(1);
                string path = Path.Combine(BaseDirectory, definition.FilePath);
                if (!File.Exists(path)) { // file not found!
                    completelySuccessful = false;
                    missings.Add(define.Name);
                    continue;
                }                
                if (definition.TextureRef != null)                
                    definition.Dispose();
                Image bmp = default;
                if (path.EndsWith(".bmp"))
                    bmp = Image.FromFile(path);
                else if (path.EndsWith(".tga"))
                    bmp = TargaImage.LoadTargaImage(path);
                definition.TextureRef = bmp;
            }
            MissingItems = missings.ToArray();
            return completelySuccessful;
        }

        public void Save(string FilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath,
                JsonSerializer.Serialize<TSOThemeFile>(this, new JsonSerializerOptions()
            {
                WriteIndented = true,
            }));
        }
    }

    public class TSOThemeFileImporter : TSOFileImporterBase<TSOThemeFile>
    {
        public static TSOThemeFile Import(string FilePath) => new TSOThemeFileImporter().ImportFromFile(FilePath);
        public override TSOThemeFile Import(Stream stream) => JsonSerializer.Deserialize<TSOThemeFile>(stream);
    }
}

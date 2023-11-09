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
        public string? FilePath { get; set; }

        [JsonIgnore]
        [TSOUIScriptEditorVisible(false)]
        public Image? TextureRef { get; internal set; }

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
        /// <summary>
        /// Loads all defined images into the <see cref="TSOThemeDefinition"/>s added to this object.
        /// <para>Note: Prior to calling this, you should ensure all <see cref="TSOThemeDefinition.FilePath"/> 
        /// are accurate and relative to the <paramref name="BaseDirectory"/></para>
        /// </summary>
        /// <param name="BaseDirectory"></param>
        public bool LoadReferencedImages(string BaseDirectory, UIScriptFile Script)
        {
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
                        continue;
                    }
                }
                catch(InvalidDataException e)
                {
                    completelySuccessful = false;
                    continue;
                }
                if (definition == null) continue;
                string path = Path.Combine(BaseDirectory, definition.FilePath);
                if (!File.Exists(path)) { // file not found!
                    completelySuccessful = false;
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

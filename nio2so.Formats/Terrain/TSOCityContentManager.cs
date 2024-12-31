using nio2so.Formats.Img.Targa;
using System.Drawing;
using System.Text.RegularExpressions;

namespace nio2so.Formats.Terrain
{
    public class TSOCityContentItem : IDisposable
    {
        /// <summary>
        /// The image data attached to this <see cref="TSOCityContentItem"/>
        /// <para>Discard this safely with: <see cref="Dispose"/> - null when not loaded.</para>
        /// </summary>
        public Image? ImageReference { get; set; } = null;

        public void Dispose()
        {
            ((IDisposable)ImageReference)?.Dispose();
            ImageReference = null;
        }
    }

    /// <summary>
    /// Manages assets loaded into memory used for displaying a <see cref="TSOCity"/>
    /// <para>See: <see cref="TSOCityImporter"/></para>
    /// </summary>
    public sealed class TSOCityContentManager : Dictionary<string, TSOCityContentItem>
    {
        /// <summary>
        /// Loads all accessible, supported Image files in the supplied directory
        /// </summary>
        /// <param name="Directory">The directory to load from</param>
        /// <returns></returns>
        public Task<(int Loaded, int TotalMatches)> LoadDirectory(DirectoryInfo Directory, string SearchPattern = "bmp|tga", bool RecursiveSubDirectories = false)
        {
            return Task.Run(delegate
            {
                if (!Directory.Exists) throw new DirectoryNotFoundException($"{Directory} does not exist!");
                //REGEX SEARCH
                var searchPattern = new Regex(@"$(?<=\.(" + SearchPattern + @"))",RegexOptions.IgnoreCase);                

                int loaded = 0, total = 0;
                foreach(var file in Directory.GetFiles("*.*", new EnumerationOptions()
                {
                    MatchType = MatchType.Win32,
                    RecurseSubdirectories = RecursiveSubDirectories
                }).Where(x => searchPattern.IsMatch(x.Name)))
                {
                    total++;
                    Image image = default;
                    string fileName = Path.GetFileNameWithoutExtension(file.Name).ToLower();
                    if (ContainsKey(fileName))
                    {
                        loaded++;
                        continue;
                    }
                    if (file.Extension.EndsWith("bmp")) // bmp importer
                        image = Image.FromFile(file.FullName);
                    else if (file.Extension.EndsWith("tga")) // tga importer
                        image = TargaImage.LoadTargaImage(file.FullName);
                    if (image == default) continue;
                    loaded++;
                    Add(fileName, new()
                    {
                        ImageReference = image
                    });
                }
                return (loaded, total);
            });
        }
    }
}

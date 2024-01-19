using System.Drawing;

namespace nio2so.Formats.Terrain
{
    /// <summary>
    /// A generic way of storing information on how to draw a texture tile
    /// </summary>
    public class TSOCityBrush
    {
        public TSOCityBrush()
        {
        }

        public TSOCityBrush(string TextureName) : this()
        {
            this.TextureName = TextureName;
        }

        /// <summary>
        /// Returns the associated <see cref="TextureName"/> from the provided <see cref="TSOCityContentManager"/>
        /// </summary>
        /// <param name="Manager"></param>
        /// <returns></returns>
        public bool TryGetTextureRef(TSOCityContentManager Manager, out Bitmap? Value)
        {
            Value = default;
            if (Manager.TryGetValue(TextureName.ToLower(), out var imageContent))
                Value = imageContent.ImageReference as Bitmap;
            return Value != default;
        }

        /// <summary>
        /// Instructs the engine displaying the mesh on what texture to use to draw this tile, if applicable in the given context.
        /// </summary>
        public string? TextureName { get; set; }
        public bool HasTexture => TextureName != null;
    }
}

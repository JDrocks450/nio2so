using nio2so.Data.Common;
using System.Drawing;

namespace nio2so.Formats.Terrain
{
    public class GeomVertex : IGeomVertex
    {
        /// <summary>
        /// The color to display when no <see cref="Brush"/> is set
        /// </summary>
        public Color Debug_TileColorHilight { get; set; }
        public GeomVector3 Position { get; }
        public GeomPoint MapPosition { get; }
        public GeomVector3 Normal { get; set; }
        public GeomPoint TextureCoord { get; }
        /// <summary>
        /// Optional vertex color to mix with the Texture to give it more color and/or detail
        /// </summary>
        public Color VertexColor { get; set; }
        /// <summary>
        /// The position in the parent <see cref="TSOCityMesh.VertexColorAtlas"/> this VertexColor resides at
        /// <para>Some platforms (like WPF) this can perform better since you're mapping a texture to a mesh and not painting verts (which WPF doesn't support directly)</para>
        /// </summary>
        public GeomPoint VertexColorAtlasCoord { get; set; }

        /// <summary>
        /// The brush implementers can use which describes how this tile should be drawn
        /// </summary>
        public TSOCityBrush? Brush { get; set; }

        public GeomVertex(GeomVector3 Position, GeomPoint MapPosition, GeomVector3 Normal, GeomPoint TextureCoord)
        {
            this.Position = Position;
            this.MapPosition = MapPosition;
            this.Normal = Normal;
            this.TextureCoord = TextureCoord;
        }
    }
}

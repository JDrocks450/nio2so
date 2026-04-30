using System.Drawing;
using System.Numerics;

namespace nio2so.Formats.Terrain.SC3K
{
    public class TSOSC3KTerrainMesh : IDisposable
    {
        internal TSOSC3KTerrainMesh(Vector3[] vertices, int[] indices, Vector2[] textureCoords, Bitmap TerrainTexture)
        {
            Vertices = vertices;
            Indices = indices;
            TextureCoords = textureCoords;
            this.TerrainTexture = TerrainTexture;
        }

        public Vector3[] Vertices { get; internal set; }
        public int[] Indices { get; internal set; }
        public Vector2[] TextureCoords { get; internal set; }
        public Vector3[] Normals { get; internal set; }
        public Bitmap? TerrainTexture { get; internal set; }

        public void Dispose()
        {
            TerrainTexture?.Dispose();
            TerrainTexture = null;
        }
    }
}

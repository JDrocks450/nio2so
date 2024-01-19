namespace nio2so.Formats.Terrain
{
    /// <summary>
    /// Sub-Meshes that share the same <see cref="TSOCityBrush"/>
    /// </summary>
    public class TSOCityBrushGeometry
    {
        /// <summary>
        /// <see cref="Vertices"/> that share the same <see cref="TSOCityBrush"/>
        /// </summary>
        public List<GeomVertex> Vertices { get; } = new();
        /// <summary>
        /// Vertex Indices that share the same <see cref="TSOCityBrush"/>
        /// </summary>
        public List<int> Indices { get; } = new();
    }
}

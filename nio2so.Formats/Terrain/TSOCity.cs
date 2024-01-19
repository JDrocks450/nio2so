using nio2so.Data.Common;
using System.Drawing;
using System.Runtime;

namespace nio2so.Formats.Terrain
{
    /// <summary>
    /// Represents a City in the Sims Online using the <see cref="TSOCityImporter"/>
    /// </summary>
    public partial class TSOCity
    {
        private static Dictionary<int, TSOCityTerrainTypes> colorMap = new()
        {
            { Color.FromArgb(255, 0, 255, 0).ToArgb(), TSOCityTerrainTypes.Grass },     //grass
            { Color.FromArgb(255, 12, 0, 255).ToArgb(), TSOCityTerrainTypes.Water },    //water
            { Color.FromArgb(255, 255, 255, 255).ToArgb(), TSOCityTerrainTypes.Snow }, //snow
            { Color.FromArgb(255, 255, 0, 0).ToArgb(), TSOCityTerrainTypes.Rock },     //rock
            { Color.FromArgb(255, 255, 255, 0).ToArgb(), TSOCityTerrainTypes.Sand },   //sand
            { Color.FromArgb(255, 0, 0, 0).ToArgb(), TSOCityTerrainTypes.Nothing }
        };

        /// <summary>
        /// The settings applied to this city
        /// </summary>
        public TSOCityConstants Settings { get; }
        /// <summary>
        /// Returns a <see cref="UtilImageIndexer"/> allowing you to read per-pixel Elevation information from the resource
        /// <para>Elevation is any color-component from the pixel read, so for example, the Red channel can be used.</para>
        /// <para>See: <see cref="GetElevationPoint(GeomPoint)"/></para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public UtilImageIndexer<double> GetElevationMap()
        {
            if (!TryGetLayer(TSOCityImporter.TSOCityDataFileTypes.ElevationMap, out var img))
                throw new NullReferenceException("Elevation Map not present!!");
            double conv(Color c) => Settings.ElevationScale * c.R;
            return new UtilImageIndexer<double>((Bitmap)img, conv);
        }
        public double GetElevationPoint(GeomPoint Position) => Settings.ElevationScale * GetElevationMap()[(int)(Position.Y * CityDataResolution.Y + Position.X)];
        public float GetElevationPoint(int x, int y) => (float)GetElevationPoint(new(x, y));

        /// <summary>
        /// Returns a <see cref="UtilImageIndexer"/> allowing you to read per-pixel Terrain information from the resource
        /// <para>See: <see cref="GetTerrainType(Color)"/></para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public UtilImageIndexer<TSOCityTerrainTypes> GetTerrainMap()
        {
            if (!TryGetLayer(TSOCityImporter.TSOCityDataFileTypes.TerrainTypeMap, out var img))
                throw new NullReferenceException("Terrain Type Map not present!!");
            static TSOCityTerrainTypes conv(Color c) => GetTerrainType(c);
            return new UtilImageIndexer<TSOCityTerrainTypes>((Bitmap)img, conv);
        }
        public static TSOCityTerrainTypes GetTerrainType(Color color) => colorMap[color.ToArgb()];

        /// <summary>
        /// Returns a <see cref="UtilImageIndexer"/> allowing you to read per-pixel Terrain information from the resource
        /// <para>See: <see cref="GetTerrainType(Color)"/></para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public UtilImageIndexer GetVertexColorMap()
        {
            if (!TryGetLayer(TSOCityImporter.TSOCityDataFileTypes.VertexColorMap, out var img))
                throw new NullReferenceException("Vertex Color map not present!!");
            return new UtilImageIndexer((Bitmap)img);
        }

        internal Dictionary<TSOCityImporter.TSOCityDataFileTypes, TSOCityContentItem> cityDataMap { get; set; } = new();
        /// <summary>
        /// Serves the image for the desired City Data layer
        /// </summary>
        /// <param name="Layer"></param>
        /// <param name="LayerRes"></param>
        /// <returns></returns>
        public bool TryGetLayer(TSOCityImporter.TSOCityDataFileTypes Layer, out Image? LayerRes) {
            LayerRes = default;
            if (cityDataMap == null) return false;
            bool success = cityDataMap.TryGetValue(Layer, out TSOCityContentItem contentItem);            
            if (!success) return false;
            LayerRes = contentItem.ImageReference;
            return true;
        }
        public bool ElevationMapPresent => TryGetLayer(TSOCityImporter.TSOCityDataFileTypes.ElevationMap, out _);
        /// <summary>
        /// The resolution of the images that make up the city data
        /// </summary>
        public GeomPoint? CityDataResolution => 
            TryGetLayer(TSOCityImporter.TSOCityDataFileTypes.TerrainTypeMap, out var img) ? new GeomPoint(img?.Width ?? 0, img?.Height ?? 0) : default;            
            
        /// <summary>
        /// The size of the City
        /// </summary>
        public GeomPoint Size => new((int)Settings.CityWidth, (int)Settings.CityHeight);        

        /// <summary>
        /// The mesh for the city geometry
        /// <para>Use: <see cref="GenerateCityMeshAsync"/></para>
        /// </summary>
        public TSOCityMesh? MeshGeometry { get; private set; }

        [Obsolete] public async Task<TSOCityMesh> GenerateCityMeshAsync() => MeshGeometry = await TSOCityGeom.GlobalDefault.GenerateMeshGeometry(this);

        public TSOCity(TSOCityConstants Settings)
        {
            this.Settings = Settings;
        }        
    }
}

using System.Drawing;

namespace nio2so.Formats.Terrain
{
    /// <summary>
    /// Defines constants for loading the selected <see cref="TSOCity"/> using the <see cref="TSOCityImporter"/>
    /// </summary>
    public class TSOCityConstants
    {
        public static TSOCityConstants Default => new();
        /// <summary>
        /// <see cref="uint.MaxValue"/> indicates to calculate the City Width automatically.
        /// </summary>
        public uint CityWidth { get; set; } = uint.MaxValue;
        /// <summary>
        /// <see cref="uint.MaxValue"/> indicates to calculate the City Height automatically.
        /// </summary>
        public uint CityHeight { get; set; } = uint.MaxValue;
        /// <summary>
        /// <see langword="default"/> indicates to automatically find the origin point
        /// </summary>
        public Point? OriginPosition { get; set; } = default;

        public double ElevationScale { get; set; } = 1/8.0;

        public int TopLeftCornerPosition { get; set; } = TSOCityImporter.TSO_CITY_TOPL;
        public int BottomLeftCornerPosition { get; set; } = TSOCityImporter.TSO_CITY_BOTL;
    }
}

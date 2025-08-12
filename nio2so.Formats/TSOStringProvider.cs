namespace nio2so.Formats
{
    /// <summary>
    /// Versions of The Sims Online
    /// </summary>
    public enum TSOVersions
    {
        Any,
        /// <summary>
        /// 1.3.1.56
        /// </summary>
        TSO_PreAlpha,
        /// <summary>
        /// 1.3.2.81
        /// </summary>
        TSO_PlayTest,
        TSO_Release
    }
    public class TSOStringProvider
    {
        public const string 
            TSO_PreAlphaName = "The Sims Online: Pre-Alpha", 
            TSO_PlayTestName = "The Sims Online: Play-Test", 
            TSO_ReleaseName = "The Sims Online: Release Build";
        private static Dictionary<TSOVersions, string> _buildNames = new Dictionary<TSOVersions, string>
        {
            { TSOVersions.TSO_PreAlpha,TSO_PreAlphaName },
            { TSOVersions.TSO_PlayTest,TSO_PlayTestName },
            { TSOVersions.TSO_Release,TSO_ReleaseName }
        };
        static string[] _mapNames =
                {
                    "Blazing Falls",
                    "Alphaville",
                    "Test Center",
                    "Interhogan",
                    "Ocean's Edge",
                    "East Jerome",
                    "Fancey Fields",
                    "Betaville",
                    "Charvatia",
                    "Dragon's Cove",
                    "Rancho Rizzo",
                    "Zavadaville",
                    "Queen Margaret's",
                    "Shannopolis",
                    "Grantley Grove",
                    "Calvin's Creek",
                    "The Billabong",
                    "Mount Fuji",
                    "Dan's Grove",
                    "Jolly Pines",
                    "Yatesport",
                    "Landry Lakes",
                    "Nichol's Notch",
                    "King Canyons",
                    "Virginia Islands",
                    "Pixie Point",
                    "West Darrington",
                    "Upper Shankelston",
                    "Alberstown",
                    "Terra Tablante"
        };
        /// <summary>
        /// Gets a name string by <paramref name="version"/>
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GetVersionName(TSOVersions version)
        {
            if (_buildNames.TryGetValue(version, out var name)) 
                return name;
            return version.ToString();
        }
        /// <summary>
        /// Get the name of the city by its ID
        /// </summary>
        /// <param name="City"></param>
        /// <returns></returns>
        public static string GetCityName(int City) => _mapNames[Math.Max(City-1,1)];
        /// <summary>
        /// Tries to <inheritdoc cref="GetCityName"/>
        /// </summary>
        /// <param name="City"></param>
        /// <param name="CityName"></param>
        /// <returns></returns>
        public static bool TryGetCityName(int City, out string? CityName) => (CityName = _mapNames.ElementAtOrDefault(Math.Max(City - 1, 1))) != default;
        /// <summary>
        /// Lists all cities in order by their ID
        /// </summary>
        /// <returns></returns>
        public static string[] GetCityNames() => _mapNames;
    }
}

using nio2so.Formats.UI.TSOTheme;
using System.Diagnostics;
using System.Drawing;

namespace nio2so.Formats.Terrain
{

    public sealed class TSOPreAlphaCityImporter : TSOCityImporter
    {
        //all of these files seem reused in both versions so this seems like a good idea.
        private static Dictionary<TSOCityDataFileTypes, string> _tsoPaCityNameDefaultMap = new()
        {
            { TSOCityDataFileTypes.ElevationMap, "elevation00.bmp" },
            { TSOCityDataFileTypes.RoadMap, "roadmap00.bmp" },
            { TSOCityDataFileTypes.TerrainTypeMap, "terraintype00.bmp" },
            { TSOCityDataFileTypes.VertexColorMap, "vertexcolor00.bmp" },
            { TSOCityDataFileTypes.ForestSprites, "forest00a.bmp" },
            { TSOCityDataFileTypes.ForestDensity, "forestdensity00.bmp" },
            { TSOCityDataFileTypes.ForestType, "foresttype00.bmp" }
        };        

        [Flags]
        public enum TSOPreAlpha_CityTileIndices : byte
        {
            AllBlend = 0,
            EastBlend = 1,
            NorthBlend = 2,
            WestBlend = 4,
            SouthBlend = 8,
        }

        public new const int TSO_CITY_SIZE = 256;
        public new const int TSO_CITY_TOPL = 152;
        public new const int TSO_CITY_BOTL = 106;

        /// <summary>
        /// The TSO Pre-Alpha City Directory
        /// <para>You can also use: <see cref="TSOCityImporter.CityDataDirectory"/></para>
        /// </summary>
        public string FarZoomDirectory { get => CityDataDirectory; set => CityDataDirectory = value; }

        /// <summary>
        /// Creates a new instance of <see cref="TSOPreAlphaCityImporter"/> with the supplied <paramref name="BaseDirectory"/>
        /// <para>Since only Blazing Falls is in Pre-Alpha. This constructor will automatically load from <paramref name="BaseDirectory"/>GameData/FarZoom</para>
        /// <para>If you want to load from a different directory, use: <see cref="TSOPreAlphaCityImporter(string, string)"/></para>
        /// </summary>
        /// <param name="BaseDirectory">The TSO Directory</param>
        public TSOPreAlphaCityImporter(string BaseDirectory) : base(BaseDirectory)
        {
            FarZoomDirectory = Path.Combine(GameDataDirectory, "FarZoom");
        }
        /// <summary>
        /// Creates a new instance of <see cref="TSOPreAlphaCityImporter"/> with the supplied <paramref name="BaseDirectory"/>
        /// <para>This constructor overrides where to pull elevation, foliage, etc. data from through the <paramref name="FarZoomDirectory"/> parameter</para>
        /// </summary>
        /// <param name="BaseDirectory">The TSO Directory</param>
        /// <param name="FarZoomDirectory">This should be a LocalPath to city elevation data, etc. -- not a relative path</param>
        public TSOPreAlphaCityImporter(string BaseDirectory, string FarZoomDirectory) : this(BaseDirectory)
        {
            this.FarZoomDirectory = FarZoomDirectory;
        }
        /// <summary>
        /// Will load all assets required to display the city scene.
        /// <para>If any assets needed are already loaded, they will be skipped. Always call this before <see cref="LoadCityAsync"/></para>
        /// </summary>
        /// <returns></returns>
        public override async Task LoadAssetsAsync(bool ContextTSOPAFileType = true)
        {
            //RESET PREVIOUS SESSION
            TSOCityDataContentItems.Clear();

            //LOAD TILESETS FOR PREALPHA
            var totalTuple = await GlobalDefaultContent.LoadDirectory(new DirectoryInfo(TerrainDirectory));
            if (totalTuple.Loaded < totalTuple.TotalMatches)
                goto error;
            //LOAD CITY DATA ITEMS
            totalTuple = await GlobalDefaultContent.LoadDirectory(new DirectoryInfo(CityDataDirectory));
            if (totalTuple.Loaded < totalTuple.TotalMatches)
                goto error;
            //SET CONTEXT TODO MAKE BETTER
            CurrentTSODataContext = ContextTSOPAFileType ? _tsoPaCityNameDefaultMap : _tsoCityNameDefaultMap;

            //MAP CONTENT ITEMS
            foreach (var ValueTuple in TSOCityFileNamesMap)
            {
                try
                {
                    TSOCityDataContentItems.Add(ValueTuple.Key, GlobalDefaultContent[Path.GetFileNameWithoutExtension(ValueTuple.Value)]);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return;
        error:
            throw new FileNotFoundException($"Only loaded {totalTuple.Loaded} / {totalTuple.TotalMatches} eligible files from the terrain directory!!");
        }

        public override Task<(TSOCity City, TSOCityMesh Mesh)> LoadCityAsync()
        {
            return Task.Run(async delegate
            {
                void throwException(string variableName) => throw new FileNotFoundException($"{variableName} Map was not found!!");

                //NEED AT LEAST ONE OF THESE FILES TO START IMPORTING
                if (!TSOCityDataContentItems.TryGetValue(TSOCityDataFileTypes.TerrainTypeMap, out TSOCityContentItem? TerrainTypeMap))
                    throwException(TSOCityDataFileTypes.TerrainTypeMap.ToString()); // THAT WAS QUICK

                //FIX SETTINGS (using the aforementioned map)
                AdjustCitySettings(TerrainTypeMap);

                TSOCity newCity = new TSOCity(CitySettings);

                //READ ELEVATION MAP
                if (!TSOCityDataContentItems.TryGetValue(TSOCityDataFileTypes.ElevationMap, out TSOCityContentItem? currentContentItem))
                    throwException(TSOCityDataFileTypes.ElevationMap.ToString());
                //SET LOADED CONTENT TO CITY OBJECT
                newCity.cityDataMap = TSOCityDataContentItems;

                Debug_ElevationBmp = new Bitmap(512, 512);

                //MAKE A GEOM PROVIDER
                TSOCityGeom geomMachine = new();
                geomMachine.OnSignalBlend += TextureRequested; // found a texture to stitch into our skin

                //GENERATE MESH
                TSOCityBrush defaultBrush = new("blank");
                _palette.Add("blank", defaultBrush);

                TSOCityMesh mesh = await geomMachine.GenerateMeshGeometry(newCity); // generate the mesh geometry                

                return (newCity, mesh);
            });
        }

        /// <summary>
        /// Generated at runtime, this is used to share a brush across multiple city tiles
        /// </summary>
        private static readonly Dictionary<string, TSOCityBrush> _palette = new();

        /// <summary>
        /// Gets the texture name
        /// </summary>
        /// <param name="City"></param>
        /// <param name="MapX"></param>
        /// <param name="MapY"></param>
        /// <param name="Edges">Up, Right, Down, Left</param>
        private void TextureRequested(TSOCity City, int MapX, int MapY, TSOCityTerrainTypes This, TSOCityTerrainTypes Other, int Flags, out TSOCityBrush Brush)
        {
            string getAbrv(TSOCityTerrainTypes Type)
            {
                return Type switch
                {
                    TSOCityTerrainTypes.Grass => "Gr",
                    TSOCityTerrainTypes.Water => "Wt",
                    TSOCityTerrainTypes.Snow => "Sn",
                    TSOCityTerrainTypes.Rock => "Rk",
                    TSOCityTerrainTypes.Sand => "Sd",
                };
            }
            string format(TSOCityTerrainTypes Me, TSOCityTerrainTypes Next, TSOPreAlpha_CityTileIndices Tile) =>
                $"{getAbrv(Next)}{getAbrv(Me)}{(byte)Tile:D2}".ToLower();

            int up = 0, right = 1, down = 2, left = 3;
            TSOCityTerrainTypes other = Other;

            if (This == TSOCityTerrainTypes.Nothing)
            {
                Brush = _palette["blank"];
                return;
            }
            TSOPreAlpha_CityTileIndices textureNum = (TSOPreAlpha_CityTileIndices)Flags;
#if false            
            if (Edges[up] != TSOCityTerrainTypes.Nothing && Edges[up] != This)
            {
                other = Edges[up];
                textureNum |= TSOPreAlpha_CityTileIndices.NorthBlend;
            }
            if (Edges[right] != TSOCityTerrainTypes.Nothing && Edges[right] != This)
            {
                textureNum |= TSOPreAlpha_CityTileIndices.EastBlend;
                other = Edges[right];
            }
            if (Edges[left] != TSOCityTerrainTypes.Nothing && Edges[left] != This)
            {
                textureNum |= TSOPreAlpha_CityTileIndices.WestBlend;
                other = Edges[left];
            }
            if (Edges[down] != TSOCityTerrainTypes.Nothing && Edges[down] != This)
            {
                textureNum |= TSOPreAlpha_CityTileIndices.SouthBlend;
                other = Edges[down];
            }
            //check if there is any change
            if (other == This) textureNum = (TSOPreAlpha_CityTileIndices)15; // no change; skip blending
#endif

            string tileTexture = "";

            if (textureNum != (TSOPreAlpha_CityTileIndices)15) // blended            
                tileTexture = format(This, other, textureNum);
            else tileTexture = format(This, This, (TSOPreAlpha_CityTileIndices)15); // Blend to nothing                

            //check if valid
            if (!GlobalDefaultContent.ContainsKey(tileTexture))
            { // invalid try again
                tileTexture = format(other, This, textureNum); // flip it
                if (!GlobalDefaultContent.ContainsKey(tileTexture))
                    tileTexture = format(This, This, (TSOPreAlpha_CityTileIndices)15); // Blend to nothing 
            }

            unchecked
            {
                byte color = (byte)((int)This * 10 + (6 * (int)other));
                Debug_ElevationBmp.SetPixel(MapX, MapY, Color.FromArgb(color, color, color));
            }

            //return full texture
            if (_palette.TryGetValue(tileTexture, out Brush)) return;
            Brush = new TSOCityBrush(tileTexture);
            _palette.Add(tileTexture, Brush);
        }

        public Bitmap Debug_ImageReadFunctionBmp { get; private set; }
        public Bitmap Debug_ElevationBmp { get; private set; }
    }

    /// <summary>
    /// A base class for importing a <see cref="TSOCity"/> from The Sims Online Pre-Alpha and The Sims Online
    /// </summary>
    public abstract class TSOCityImporter
    {
        public const int TSO_CITY_SIZE = 512;
        public const int TSO_CITY_TOPL = 306;
        public const int TSO_CITY_BOTL = 205;

        /// <summary>
        /// The default <see cref="TSOCityContentManager"/> instance
        /// </summary>
        public static TSOCityContentManager GlobalDefaultContent { get; } = new();

        protected TSOCityImporter(string BaseDirectory) : this(BaseDirectory, TSOCityConstants.Default) { }                          
        protected TSOCityImporter(string BaseDirectory, TSOCityConstants CitySettings)
        {
            this.BaseDirectory = BaseDirectory;
            this.CitySettings = CitySettings;
        }
        /// <summary>
        /// TSO Game Directory
        /// </summary>
        public string BaseDirectory { get; }
        /// <summary>
        /// Gets or sets the settings to use when importing the selected city
        /// </summary>
        public TSOCityConstants CitySettings { get; set; }

        /// <summary>
        /// The folder containing the assets for the City selected for viewing.
        /// <para>For TSO New Improved, it would be in the <c>cities</c> directory, in TSO Pre-Alpha this is <c>FarZoom</c></para>
        /// </summary>
        public string CityDataDirectory { get; protected set; }
        public TSOThemeFile TSOThemeFile { get; private set; }

        /// <summary>
        /// The TSO GameData directory
        /// </summary>
        public string GameDataDirectory => Path.Combine(BaseDirectory, "GameData");
        /// <summary>
        /// The TSO GameData\Terrain directory
        /// </summary>
        public string TerrainDirectory => Path.Combine(GameDataDirectory, "Terrain");
        /// <summary>
        /// Known types of files needed to display the <see cref="TSOCity"/>
        /// </summary>
        public enum TSOCityDataFileTypes
        {
            /// <summary>
            /// Elevation00.bmp
            /// </summary>
            ElevationMap,
            /// <summary>
            /// ForestDensity00.bmp
            /// </summary>
            ForestDensity,
            /// <summary>
            /// ForestType00.bmp
            /// </summary>
            ForestType,
            /// <summary>
            /// RoadMap00.bmp
            /// </summary>
            RoadMap,
            /// <summary>
            /// TerrainType00.bmp
            /// </summary>
            TerrainTypeMap,
            /// <summary>
            /// VertexColor00.bmp
            /// </summary>
            VertexColorMap,
            /// <summary>
            /// Forest00A.bmp
            /// </summary>
            ForestSprites
        }
        
        protected static Dictionary<TSOCityDataFileTypes, string> _tsoCityNameDefaultMap = new()
        {
            { TSOCityDataFileTypes.ElevationMap, "elevation.bmp" },
            { TSOCityDataFileTypes.RoadMap, "roadmap.bmp" },
            { TSOCityDataFileTypes.TerrainTypeMap, "terraintype.bmp" },
            { TSOCityDataFileTypes.VertexColorMap, "vertexcolor.bmp" },
            { TSOCityDataFileTypes.ForestSprites, "forest00a.bmp" },
            { TSOCityDataFileTypes.ForestDensity, "forestdensity.bmp" },
            { TSOCityDataFileTypes.ForestType, "foresttype.bmp" }
        };
        protected Dictionary<TSOCityDataFileTypes, string> CurrentTSODataContext { get; set; } = _tsoCityNameDefaultMap;
        /// <summary>
        /// Maps types of files used in rendering a <see cref="TSOCity"/> to file names in the TSO installation directory
        /// </summary>
        protected virtual Dictionary<TSOCityDataFileTypes, string> TSOCityFileNamesMap => CurrentTSODataContext;
        /// <summary>
        /// Maps types of files to the content item the asset is connected to
        /// </summary>
        protected virtual Dictionary<TSOCityDataFileTypes, TSOCityContentItem> TSOCityDataContentItems { get; } = new();

        public virtual void SetTheme(TSOThemeFile TSOThemeFile) => this.TSOThemeFile = TSOThemeFile;

        protected virtual void AdjustCitySettings(TSOCityContentItem terrainTypeMap)
        {
            Bitmap? imgRef = (Bitmap)terrainTypeMap.ImageReference;
            if (imgRef == default)
                throw new NullReferenceException(nameof(imgRef));

            Point getCityOriginPointAuto()
            {
                for (int y = 0; y < imgRef.Height; y++) // search from top down
                {
                    for (int x = 0; x < imgRef.Width; x++)
                    {
                        var pixel = imgRef.GetPixel(x, y);
                        if (pixel.Name == "ff000000") continue;
                        return new Point(x, y);
                    }
                }
                //IMAGE IS BLANK
                throw new InvalidDataException("The TerrainType map provided is blank.");
            }
            uint getCityWidthAuto()
            {
                for (int x = 0; x < imgRef.Width; x++) // search from left bound ->
                {
                    for (int y = 0; y < imgRef.Height; y++)
                    {
                        var pixel = imgRef.GetPixel(x, y);
                        if (pixel.Name == "ff000000") continue;
                        return (uint)(Math.Abs((CitySettings.OriginPosition?.X ?? 0) - x) * Math.Sqrt(2)); // 45 degree line makes it easy
                    }
                }
                //IMAGE IS BLANK
                throw new InvalidDataException("The TerrainType map provided is blank.");
            }
            uint getCityHeightAuto()
            {
                for (int x = imgRef.Width - 1; x >= 0; x--) // search from right bound <-
                {
                    for (int y = 0; y < imgRef.Height; y++)
                    {
                        var pixel = imgRef.GetPixel(x, y);
                        if (pixel.Name == "ff000000") continue;
                        return (uint)(Math.Abs((CitySettings.OriginPosition?.X ?? 0) - x) * Math.Sqrt(2)); // 45 degree line makes it easy
                    }
                }
                //IMAGE IS BLANK
                throw new InvalidDataException("The TerrainType map provided is blank.");
            }

            if (CitySettings.OriginPosition == default)
                CitySettings.OriginPosition = getCityOriginPointAuto();
            if (CitySettings.CityWidth == uint.MaxValue)
                CitySettings.CityWidth = getCityWidthAuto();
            if (CitySettings.CityHeight == uint.MaxValue)
                CitySettings.CityHeight = getCityHeightAuto();
            if (imgRef.Width < 257) // TSO PREALPHA
            {
                CitySettings.TopLeftCornerPosition = TSOPreAlphaCityImporter.TSO_CITY_TOPL;
                CitySettings.BottomLeftCornerPosition = TSOPreAlphaCityImporter.TSO_CITY_BOTL;
            }
            else if (imgRef.Width == 512) // TSO N&I
            {
                CitySettings.TopLeftCornerPosition = TSO_CITY_TOPL;
                CitySettings.BottomLeftCornerPosition = TSO_CITY_BOTL;
            }
        }

        public abstract Task LoadAssetsAsync(bool ContextTSOPAFileType = true);

        public abstract Task LoadCityAsync();
    }
}

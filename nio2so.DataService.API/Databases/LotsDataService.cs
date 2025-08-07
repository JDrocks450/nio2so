using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using nio2so.DataService.Common.Types.Search;

namespace nio2so.DataService.API.Databases
{
    internal class LotsDataService : DataServiceBase, ISearchable<uint>
    {
        JSONDictionaryLibrary<uint, LotInfo> LotsLibrary => GetLibrary<JSONDictionaryLibrary<uint, LotInfo>>("LOTS");

        public LotsDataService() : base()
        {

        }

        protected override void AddLibraries()
        {
            ServerSettings settings = ServerSettings.Current;

            //thumbnail database
            Libraries.Add("THUMBNAILS", new FileObjectLibrary(Path.Combine(settings.DatabaseDirectory, "thumbnails"),
                "thumb", "png", GetDefaultThumbnail));
            //lot database
            Libraries.Add("LOTS", new JSONDictionaryLibrary<uint, LotInfo>(settings.LotInfoFile, EnsureDefaultLots));
            //**creation index for lot IDs
            Libraries.Add("LOT CREATION INDEX", new JSONCreationIndex(settings.LotCreationIndexFile));
            //**library for houseblobs
            Libraries.Add("HOUSEBLOBS", new FileObjectLibrary(settings.HouseBlobLibraryDirectory, "house", "houseblob", GetDefaultHouseBlob));

            base.AddLibraries();
        }

        Task<byte[]> GetDefaultHouseBlob() => File.ReadAllBytesAsync(ServerSettings.Current.DefaultHouseBlobPath);
        Task<byte[]> GetDefaultThumbnail() => File.ReadAllBytesAsync(ServerSettings.Current.DefaultThumbnailPath);

        Task EnsureDefaultLots()
        {
            ServerSettings settings = ServerSettings.Current;

            // add the static lots to the database and skip if existing
            foreach(var lot in settings.StaticLots)
                LotsLibrary.TryAdd(lot.HouseID, new LotInfo() { Profile = lot });
            return Save();
        }

        /// <summary>
        /// Returns the PNG Image for the given <see cref="HouseIDToken"/> in binary
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task<byte[]> GetThumbnailByHouseID(HouseIDToken HouseID) => 
            GetLibrary<FileObjectLibrary>("THUMBNAILS").GetDataByID(HouseID);
        /// <summary>
        /// Sets the PNG Image for the given <see cref="HouseIDToken"/> in binary
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task GetThumbnailByHouseID(HouseIDToken HouseID, byte[] PNGBytes) =>
            GetLibrary<FileObjectLibrary>("THUMBNAILS").SetDataByIDToDisk(HouseID,PNGBytes);
        /// <summary>
        /// Sets the PNG Image for the given <see cref="HouseIDToken"/> in binary
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task SetThumbnailByHouseID(HouseIDToken HouseID, byte[] PNGStream, bool overwrite = true) =>
            GetLibrary<FileObjectLibrary>("THUMBNAILS").SetDataByIDToDisk(HouseID, PNGStream, overwrite);
        /// <summary>
        /// Returns the <see cref="LotProfile"/> with the given <see cref="HouseIDToken"/> <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public LotProfile GetLotProfileByLotID(HouseIDToken HouseID)
        {
            if (!LotsLibrary.ContainsKey(HouseID))
                throw new KeyNotFoundException($"The HouseID: {HouseID} was not found in the data service.");
            return LotsLibrary[HouseID].Profile;
        }
        /// <summary>
        /// Gets all lots in this database
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LotProfile> GetLots() => LotsLibrary.Select(x => x.Value.Profile);
        /// <summary>
        /// Changes a field on the <see cref="LotProfile"/> by <paramref name="houseID"/>
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> SetLotProfileFields(HouseIDToken houseID, string field, string value)
        {
            field = field.ToLowerInvariant();
            var profile = GetLotProfileByLotID(houseID);

            switch (field)
            {
                case "name":
                    profile.Name = value;
                    await Save();
                    return true;
                case "description":
                    profile.Description = value;
                    await Save();
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Uses the Lot Creation Index to get the next available ID and returns it
        /// <para/>Is not reserved until <see cref="TryPurchaseLotByAvatarID(AvatarIDToken, TSODBChar, string, LotPosition, out LotProfile?)"/> is called (which in turn will call this)
        /// <para/>Every time this is rolled, the Lot Creation Index is incremented to the next ID
        /// </summary>
        /// <returns></returns>
        public void UpdateHouseCreationIndex(HouseIDToken HouseID)
        {
            JSONCreationIndex lib = GetLibrary<JSONCreationIndex>("LOT CREATION INDEX");
            lib.PushBack(HouseID);
        }

        /// <summary>
        /// Tries to purchase the new lot for the provided <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarProfile"></param>
        /// <param name="Position"></param>
        /// <param name="NewLotProfile"></param>
        /// <returns></returns>
        public async Task<(bool Success, string Reason, LotProfile? NewLotProfile)> TryPurchaseLotByAvatarID(AvatarIDToken AvatarID, HouseIDToken HouseID, TSODBChar AvatarProfile, LotPosition Position)
        {
            LotProfile? NewLotProfile = null;
            string Reason = "success.";

            if (AvatarProfile.Funds <= ServerSettings.Current.LotPurchasePrice)
                return (false, "You don't have enough money.", null); // Refused! You're broke!

            if (LotsLibrary.ContainsKey(HouseID))
                return (false, "That lot was just bought recently. Sorry!", null); // Refused! Someone lives here!

            bool result = LotsLibrary.TryAdd(HouseID, new LotInfo()
            {
                Profile = NewLotProfile = new(HouseID, AvatarID, Position, $"{AvatarProfile.AvatarName}'s House",
                $"Created on {DateTime.Now.ToShortDateString()}.\n\nEnter a cool description here...")
            });

            if (result)
            {
                await GetLibrary<FileObjectLibrary>("HOUSEBLOBS").SetDataByIDToDisk(HouseID, await GetDefaultHouseBlob());
                AvatarProfile.Funds -= ServerSettings.Current.LotPurchasePrice;
                UpdateHouseCreationIndex(HouseID);
            }
            await Save(); // save the db
            return (true, Reason, NewLotProfile);
        }

        /// <summary>
        /// Returns a list of all roommates (owner included) by the given <see cref="HouseIDToken"/> <paramref name="HouseID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public IEnumerable<AvatarIDToken> GetRoommatesByHouseID(HouseIDToken HouseID)
        {
            if (!LotsLibrary.TryGetValue(HouseID, out LotInfo? info))
                throw new KeyNotFoundException(HouseID.ToString());
            return info.GetRoommates();
        }

        /// <summary>
        /// Gets the <see cref="HouseIDToken"/> that this <paramref name="AvatarID"/> is a member of (roommate, owner)
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public HouseIDToken? GetRoommateHouseIDByAvatarID(AvatarIDToken AvatarID)
        {
            lock (LotsLibrary)
            {
                foreach(var house in LotsLibrary)
                {
                    if (house.Value.GetRoommates().Contains(AvatarID)) return house.Key;
                }
                return null;
            }
        }

        public IDictionary<uint, string> SearchExact(string QueryString) => LotsLibrary.SearchExact(QueryString);
        public IDictionary<uint, string> SearchBroad(string QueryString, int MaxResults) => LotsLibrary.SearchBroad(QueryString, MaxResults);
        /// <summary>
        /// Saves the provided HouseBlob to the HOUSEBLOB library
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task SetHouseBlobByHouseID(HouseIDToken houseID, byte[] data)
        {
            if (!LotsLibrary.TryGetValue(houseID, out LotInfo info))
                throw new KeyNotFoundException(houseID + " is not found in the LotLibrary.");
            if (info.IsReadOnly)
                throw new InvalidOperationException(houseID + " is " + nameof(LotInfo.IsReadOnly) + " = true. Changes were not saved.");
            return GetLibrary<FileObjectLibrary>("HOUSEBLOBS").SetDataByIDToDisk(houseID, data);
        }
        /// <summary>
        /// Reads the HouseBlob in the HOUSEBLOB library at the <paramref name="houseID"/>
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<byte[]> GetHouseBlobByHouseID(HouseIDToken houseID) => GetLibrary<FileObjectLibrary>("HOUSEBLOBS").GetDataByID(houseID);
    }
}

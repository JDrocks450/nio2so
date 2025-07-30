using Microsoft.AspNetCore.Mvc;
using nio2so.Data.Common.Testing;
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

            base.AddLibraries();
        }

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
        public bool SetLotProfileFields(HouseIDToken houseID, string field, string value)
        {
            field = field.ToLowerInvariant();
            var profile = GetLotProfileByLotID(houseID);

            switch (field)
            {
                case "name":
                    profile.Name = value;
                    Save();
                    return true;
                case "description":
                    profile.Description = value;
                    Save();
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
        public HouseIDToken GetNextID()
        {
            JSONCreationIndex lib = GetLibrary<JSONCreationIndex>("LOT CREATION INDEX");
            return lib.GetNextID(LotsLibrary);
        }

        /// <summary>
        /// Tries to purchase the new lot for the provided <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarProfile"></param>
        /// <param name="LotPhoneNumber"></param>
        /// <param name="Position"></param>
        /// <param name="NewLotProfile"></param>
        /// <returns></returns>
        public bool TryPurchaseLotByAvatarID(AvatarIDToken AvatarID, TSODBChar AvatarProfile, string LotPhoneNumber, LotPosition Position, out LotProfile? NewLotProfile)
        {
            HouseIDToken HouseID;
            NewLotProfile = null;

            if (AvatarProfile.Funds <= ServerSettings.Current.LotPurchasePrice)
                return false; // Refused! You're broke!

            if (LotsLibrary.Values.Any(x => x.Profile.PhoneNumber == LotPhoneNumber))
                return false; // Refused! Someone lives here!

            do
            { // set houseid to next ID and update creation index
                HouseID = GetNextID();
            }
            while (!LotsLibrary.TryAdd(HouseID, new LotInfo()
            {
                Profile = NewLotProfile = new(HouseID, AvatarID, Position, LotPhoneNumber, $"{AvatarProfile.AvatarName}'s House",
                $"Created on {DateTime.Now.ToShortDateString()}.\n\nEnter a cool description here...")
            }));

            AvatarProfile.Funds -= ServerSettings.Current.LotPurchasePrice;
            Save(); // save the db
            return NewLotProfile != null;
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
    }
}

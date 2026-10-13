using Microsoft.AspNetCore.Mvc;
using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Lot;
using nio2so.DataService.Common.Types.Top100;
using System.Text;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// This Data Service component serves data on the Top 100 List functionality in nio2so
    /// </summary>
    public class Top100DataService : DataServiceBase
    {
        const string SERVICE_NAME = "Top100DataService";
        const string LIB_NAME = "TOP100";

        private string? repairSummary = null;
        /// <summary>
        /// The <see cref="Top100DataService"/> can repair the dataset if it is invalid, and this will summarize the changes.
        /// </summary>
        /// <returns></returns>
        public string? GetRepairSummary()
        {
            string foo = repairSummary;
            repairSummary = null;
            return foo;
        }

        public Top100DataService() : base() {
        
        }

        protected override void AddLibraries()
        {
            ServerSettings settings = CurrentSettings;
            Libraries.Add(LIB_NAME, new JSONDictionaryLibrary<uint, Top100ListInfo>(settings.DereferencePath(settings.Top100ListsLibraryPath),EnsureDefaultLists));

            base.AddLibraries();
        }
        
        /// <summary>
        /// Gets all <see cref="Top100DataService"/> definitions added to this DataService library.
        /// </summary>
        /// <returns></returns>
        internal Task<IEnumerable<Top100ListInfo>> GetTop100Lists() => 
            Task.FromResult(GetLibrary<JSONDictionaryLibrary<uint, Top100ListInfo>>(LIB_NAME).Dictionary.Values.AsEnumerable());

        /// <summary>
        /// Emplaces a default set of <see cref="Top100ListInfo"/> into the library if it's empty. 
        /// This is intended to provide a template for what a top 100 list definition looks like,
        /// and to ensure that there is always at least one top 100 list available for remote connections to query and display.
        /// </summary>
        /// <returns></returns>
        Task EnsureDefaultLists()
        {
            var top100list = GetLibrary<JSONDictionaryLibrary<uint, Top100ListInfo>>(LIB_NAME);

            if (top100list.Count == 0)
            {
                //General-use template showing each category of top 100 list

                string iconResource = @"C:\nio2so\const\top100_1.bmp";
                top100list.Dictionary.TryAdd(1, new Top100ListInfo(1001, "Avatars", "My Top Avatars", iconResource));
                //1002 is hardcoded to be Most Popular Places list -- this one reflects that
                top100list.Dictionary.TryAdd(2, new Top100ListInfo(1002, "Houses", "Splash House Zone", iconResource));
                top100list.Dictionary.TryAdd(3, new Top100ListInfo(1003, "Clubs", "questionable club blt", iconResource));
                top100list.Dictionary.TryAdd(4, new Top100ListInfo(1004, "Neighborhoods", "top neighborhoods", iconResource));
            }

            RepairEntries();

            return Save();
        }

        /// <summary>
        /// Ensures the <see cref="Top100ListInfo.ListID"/> property matches that stored in the Library.
        /// </summary>
        private void RepairEntries()
        {
            StringBuilder repairSummaryBuilder = new("===TOP 100 SERVICE Repair Log===\n");

            Dictionary<uint, Top100ListInfo> fooList = new();
            List<Top100ListInfo> errors = new();
            var top100lists = GetLibrary<JSONDictionaryLibrary<uint, Top100ListInfo>>(LIB_NAME);

            //check if any sorting is necessary
            bool errorFound = false;
            foreach(var item in top100lists)
            {
                if (item.Key != item.Value.ListID)
                {
                    errorFound = true;
                    break;
                }
            }

            if (!errorFound) return; // no errors

            //errors found, fix them
            repairSummaryBuilder.AppendLine($"Entries were found with ListIDs mis-matched. They have been corrected without errors.");

            // move all entries to a new dictionary that maps all ListIDs to keys
            foreach (var list in top100lists)
            {
                if (!fooList.TryAdd(list.Value.ListID, list.Value))
                    errors.Add(list.Value); // duplicate key!
            }

            //we will now give new ListIDs to any duplicates

            uint highest = fooList.Keys.Max();
            if (highest < 1000) highest = 1000; // start from 1000
            //count up from the highest found id for all new ListIDs
            foreach(var list in errors)
            {
                highest++;
                uint oldListID = list.ListID;
                uint newListID = highest;
                list.ListID = newListID;
                fooList.Add(newListID, list);
                repairSummaryBuilder.AppendLine($"List: {oldListID} has been moved to {newListID} because it had the same ID as another list.");
            }

            top100lists.Clear();

            foreach (var list in fooList)
                top100lists.Add(list);

            repairSummary = repairSummaryBuilder.ToString();
        }

        /// <summary>
        /// Returns the items contained in a <see cref="Top100ListInfo"/>
        /// </summary>
        /// <param name="ListID"></param>
        /// <returns></returns>
        internal Top100ListItemsInfo? GetItemsByListID(uint ListID)
        {
            if (!GetLibrary<JSONDictionaryLibrary<uint, Top100ListInfo>>(LIB_NAME).TryGetValue(ListID, out var top100list))
                return null; // NOT_FOUND

            HashSet<nio2soTop100Item> DataSet = new HashSet<nio2soTop100Item>();

            switch (top100list.ListType.ToLowerInvariant())
            {
                case "avatars":
                    {
                        //get sample data set

                        IEnumerable<AvatarInfo> dataSource = APIDataServices.AvatarDataService.GetAllProfiles();

                        //sort all avatars by simoleans
                        uint rank = 0;
                        foreach (var item in dataSource.OrderByDescending(x => x.Profile.Simoleans))
                            DataSet.Add(new nio2soTop100Item(++rank, item.AvatarID, item.AvatarName, $"${item.Profile.Simoleans}"));
                        //return dataset
                    }
                    break;
                case "houses":
                    {
                        //get sample data set

                        IEnumerable<LotProfile> dataSource = APIDataServices.LotDataService.GetLots();

                        //sort all lots
                        uint rank = 0;
                        foreach (var item in dataSource)
                            DataSet.Add(new nio2soTop100Item(++rank, item.HouseID, item.Name, ""));
                        //return dataset
                    }
                    break;
            }
            return new Top100ListItemsInfo(top100list.ListID, top100list.ListType, top100list.ListName, DataSet);
        }
    }
}

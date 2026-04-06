using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Top100;

namespace nio2so.DataService.API.Databases
{
    public class Top100DataService : DataServiceBase
    {
        const string SERVICE_NAME = "Top100DataService";
        const string LIB_NAME = "TOP100";

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
        public Task<IEnumerable<Top100ListInfo>> GetTop100Lists() => 
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

            if (top100list.Count != 0) return Task.CompletedTask;

            //General-use template showing each category of top 100 list
            
            string iconResource = @"C:\nio2so\const\top100_1.bmp";
            top100list.Dictionary.TryAdd(1, new Top100ListInfo(1, "Avatars", "My Top Avatars", iconResource));
            top100list.Dictionary.TryAdd(2, new Top100ListInfo(2, "Houses", "Splash House Zone", iconResource));
            top100list.Dictionary.TryAdd(3, new Top100ListInfo(3, "Clubs", "questionable club blt", iconResource));
            top100list.Dictionary.TryAdd(4, new Top100ListInfo(4, "Neighborhoods", "top neighborhoods", iconResource));

            return Save();
        }
    }
}

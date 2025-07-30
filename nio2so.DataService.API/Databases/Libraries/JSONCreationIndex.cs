using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API.Databases.Libraries
{
    internal class JSONCreationIndex : JSONObjectLibrary<HashSet<uint>> 
    {
        public static uint StartingValue { get; set; } = 100;

        public JSONCreationIndex(string FilePath, Func<Task>? EnsureDefaultValuesFunc = null, bool DelayedLoad = false) : base(FilePath, EnsureDefaultValuesFunc, DelayedLoad)
        {
        }

        /// <summary>
        /// Uses the Lot Creation Index to get the next available ID and returns it
        /// <para/>Is not reserved until <see cref="TryPurchaseLotByAvatarID(AvatarIDToken, TSODBChar, string, LotPosition, out LotProfile?)"/> is called (which in turn will call this)
        /// <para/>Every time this is rolled, the Lot Creation Index is incremented to the next ID
        /// </summary>
        /// <returns></returns>
        public uint GetNextID<T>(IDictionary<uint, T> DataSource)
        {
            uint newID;
            if (DataFile.Any()) // see if the creation index has any content
                newID = DataFile.Last();
            else if (DataSource.Any())
                newID = DataSource.Keys.First(); // a bit slow, but allows us to fill in the gaps between values
            else newID = StartingValue;
            do
            { // increment up once
                DataFile.Add(++newID); // update lot creation index
            }
            while (DataSource.ContainsKey(newID)); // see if this is an available ID
            Save();
            return newID;
        }
    }
}

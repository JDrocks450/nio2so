using nio2so.DataService.Common.Types.Lot;

namespace nio2so.DataService.Common.Queries
{
    public class N2GetLotListQueryResult
    {
        public N2GetLotListQueryResult()
        {

        }

        public N2GetLotListQueryResult(string ShardName, IEnumerable<LotListEntry> Entries) : this()
        {
            this.ShardName = ShardName;
            LotCount = Entries.Count();
            Lots = Entries;
        }

        public record LotListEntry(uint HouseID, LotPosition Position);
        public string ShardName { get; set; }
        public int LotCount { get; set; }  
        public IEnumerable<LotListEntry> Lots { get; set; }
    }
}

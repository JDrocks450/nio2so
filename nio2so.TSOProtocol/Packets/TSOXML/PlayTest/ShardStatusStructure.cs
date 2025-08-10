using nio2so.Data.Common.Testing;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PlayTest
{
    public record struct ShardStatusStructure(string Name,
                                              int Rank,
                                              string Location,
                                              [property:TSOXMLElementName("Online-Avatars")] uint OnlineAvatars,
                                              ShardStatusReason Status,
                                              int Map) : IVersionedPacketStructure                                                                                            
    {

        public static ShardStatusStructure Default = new(Name: TestingConstraints.MyShardName, Rank: 1, Location: "LOCATION", OnlineAvatars: 1, Status: ShardStatusReason.Up, Map: 10);

        public ShardStatusStructure(string name, int rank, uint onlineAvatars, int map) : this(name, rank, "LOCATION", onlineAvatars, ShardStatusReason.Up, map)
        {
            Name = name;
            Rank = rank;
            OnlineAvatars = onlineAvatars;
            Map = map;
        }

        public IVersionedPacketStructure GetDefault() => Default;
    }
}

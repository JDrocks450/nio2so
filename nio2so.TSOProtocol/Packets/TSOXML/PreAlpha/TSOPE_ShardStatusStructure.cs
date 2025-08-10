using nio2so.Data.Common.Testing;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha
{
    public record struct TSOPE_ShardStatusStructure(string Location,
                                              string Name,
                                              ShardStatusReason Status,
                                              [property:TSOXMLElementName("Online-Avatars")] uint OnlineAvatars) : IVersionedPacketStructure
    {
        public TSOPE_ShardStatusStructure(ShardLocation Location, string Name,
            ShardStatusReason Status, uint OnlineAvatars) : this(Location.ToString().ToLower(), Name, Status, OnlineAvatars) { }

        public static TSOPE_ShardStatusStructure Default = new(ShardLocation.PUBLIC, TestingConstraints.MyShardName, ShardStatusReason.Up, 1);

        public IVersionedPacketStructure GetDefault() => Default;
    }
}

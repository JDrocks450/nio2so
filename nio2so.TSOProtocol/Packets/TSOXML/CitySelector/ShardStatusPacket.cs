using nio2so.Data.Common.Testing;
using System.Xml.Linq;

namespace nio2so.TSOProtocol.Packets.TSOXML.CitySelector
{
    public enum ShardLocation
    {
        PUBLIC, PRIVATE       
    }

    public enum ShardStatusReason
    {
        Up,
        Down
    }

    public record struct ShardStatusStructure(string Location,
                                              string Name,
                                              ShardStatusReason Status,
                                              [property:TSOXMLElementName("Online-Avatars")] uint OnlineAvatars) : ITSOXMLStructure
    {
        public ShardStatusStructure(ShardLocation Location, string Name,
            ShardStatusReason Status, uint OnlineAvatars) : this(Location.ToString().ToLower(), Name, Status, OnlineAvatars) { }

        public static ShardStatusStructure Default = new(ShardLocation.PUBLIC, TestingConstraints.MyShardName, ShardStatusReason.Up, 1);
    }

    public class ShardStatusPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorShardNode = "Shard-Status-List", TSOCitySelectorShardElement = "Shard-Status";

        public ShardStatusPacket(params ShardStatusStructure[] Shards) : base(TSOCitySelectorShardNode)
        {
            foreach (var Shard in Shards)
            {
                XElement ShardData = new XElement(TSOCitySelectorShardElement);
                SerializeXML(Shard, ShardData);
                RootElement.Add(ShardData);
            }
        }
    }
}

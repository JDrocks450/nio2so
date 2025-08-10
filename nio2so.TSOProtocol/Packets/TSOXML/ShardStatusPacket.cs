using System.Xml.Linq;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML
{
    public enum ShardLocation
    {
        PUBLIC,
        /// <summary>
        /// "dev-tuning"
        /// </summary>
        DEV_TUNING, PRIVATE       
    }

    public enum ShardStatusReason
    {
        Up,
        Down
    }

    public class ShardStatusPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorShardNode = "Shard-Status-List", TSOCitySelectorShardElement = "Shard-Status";

        public ShardStatusPacket(params IVersionedPacketStructure[] Shards) : base(TSOCitySelectorShardNode)
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

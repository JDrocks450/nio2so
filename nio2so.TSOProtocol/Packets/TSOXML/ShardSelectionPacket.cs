using nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML
{

    public class ShardSelectionPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorShardSelection = "Shard-Selection";

        public ShardSelectionPacket(ITSOXMLStructure Shard) : base(TSOCitySelectorShardSelection)
        {
            SerializeXML(Shard);
        }
    }
}

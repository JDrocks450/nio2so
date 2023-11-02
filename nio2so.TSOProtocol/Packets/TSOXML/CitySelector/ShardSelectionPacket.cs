using nio2so.Protocol.Data.Credential;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOProtocol.Packets.TSOXML.CitySelector
{
    public record struct ShardSelectionStructure([property:TSOXMLElementName("Connection-Address")] string ConnectionAddress,
                                                 [property: TSOXMLElementName("Authorization-Ticket")] TSOSessionTicket AuthorizationTicket,
                                                 uint PlayerID) : ITSOXMLStructure;

    public class ShardSelectionPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorShardSelection = "Shard-Selection";

        public ShardSelectionPacket(ShardSelectionStructure Shard) : base(TSOCitySelectorShardSelection)
        {
            MakePacket(Shard);
        }
    }
}

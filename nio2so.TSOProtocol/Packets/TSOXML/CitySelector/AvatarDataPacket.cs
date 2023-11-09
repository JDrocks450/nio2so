using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.TSOProtocol.Packets.TSOXML.CitySelector
{
    public record struct AvatarDataPacketStructure(ulong AvatarID,
                                                   string Name,
                                                   long Simoleans,
                                                   [property: TSOXMLElementName("Simolean-Delta")] long SimoleanDelta,
                                                   byte Popularity,
                                                   [property: TSOXMLElementName("Popularity-Delta")] byte PopularityDelta,
                                                   [property: TSOXMLElementName("Shard-Name")] string ShardName) : ITSOXMLStructure;

    public class AvatarDataPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorAvatarDataBase = "The-Sims-Online", TSOCitySelectorAvatarDataElement = "Avatar-Data";

        public static AvatarDataPacketStructure Default = new()
        {
            AvatarID = 0x0539,
            Name = "Bloaty",
            Popularity = 5,
            PopularityDelta = 1,
            ShardName = "Blazing Falls",
            SimoleanDelta = 5000,
            Simoleans = 20000
        };
        public static AvatarDataPacketStructure Empty = new()
        {
            AvatarID = 0x0,
            Name = "\0",
            Popularity = 0,
            PopularityDelta = 0,
            ShardName = "Blazing Falls",
            SimoleanDelta = 0,
            Simoleans = 0
        };

        public AvatarDataPacket(params AvatarDataPacketStructure[] Avatars) : base(TSOCitySelectorAvatarDataBase)
        {
            foreach (var Avatar in Avatars)
            {
                XElement AvatarData = new XElement(TSOCitySelectorAvatarDataElement);
                MakePacket(Avatar, AvatarData);
                RootElement.Add(AvatarData);
                RootElement.Add(new XElement("Player-Active"));
            }
        }
    }
}

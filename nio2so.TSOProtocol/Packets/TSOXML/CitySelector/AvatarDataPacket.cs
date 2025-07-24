using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types.Avatar;
using System.Xml.Linq;

namespace nio2so.TSOProtocol.Packets.TSOXML.CitySelector
{ 
    public class AvatarDataPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorAvatarDataBase = "The-Sims-Online", TSOCitySelectorAvatarDataElement = "Avatar-Data";

        public static AvatarProfile Default => new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName,TestingConstraints.StartingFunds,5000, 5, 1, TestingConstraints.MyShardName);

        public static AvatarProfile Empty = AvatarProfile.Empty;

        public AvatarDataPacket(params AvatarProfile[] Profiles) : base(TSOCitySelectorAvatarDataBase)
        {
            foreach (var Avatar in Profiles)
            {
                XElement AvatarData = new XElement(TSOCitySelectorAvatarDataElement);
                SerializeXML(Avatar, AvatarData);
                RootElement.Add(AvatarData);                                
            }
            RootElement.Add(new XElement("Player-Active"));
        }
    }
}

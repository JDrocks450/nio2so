using System.Xml.Linq;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML
{ 
    public class AvatarDataPacket<T> : TSOXMLPacket
    {
        public const string TSOCitySelectorAvatarDataBase = "The-Sims-Online", TSOCitySelectorAvatarDataElement = "Avatar-Data";              

        public AvatarDataPacket(T[] Profiles, params string[] AppendTags) : base(TSOCitySelectorAvatarDataBase)
        {
            foreach (var Avatar in Profiles)
            {
                XElement AvatarData = new XElement(TSOCitySelectorAvatarDataElement);
                SerializeXML(Avatar, AvatarData);
                RootElement.Add(AvatarData);                                
            }
            foreach(var tag in AppendTags) 
                RootElement.Add(new XElement(tag));
        }
    }
}

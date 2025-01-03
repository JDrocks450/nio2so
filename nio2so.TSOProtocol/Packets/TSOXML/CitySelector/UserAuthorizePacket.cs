using System.Xml.Linq;

namespace nio2so.TSOProtocol.Packets.TSOXML.CitySelector
{
    /// <summary>
    /// The User-Authorize Packet. 
    /// <para>It is very basic, with only one xml element.</para>
    /// </summary>
    public class UserAuthorizePacket : TSOXMLPacket
    {
        public const string TSOCitySelectorUserAuthorize = "User-Authorized";
        public const string TSOCitySelectorUserCSRAuthorize = "csr";
        /// <summary>
        /// Makes a new <see cref="UserAuthorizePacket"/> optionally with the user being a CSR.
        /// <para>TSO PreAlpha is not compatible with this and nio2so will automatically disable it. </para>
        /// </summary>
        /// <param name="CSR"></param>
        public UserAuthorizePacket(bool CSR = false) : base(TSOCitySelectorUserAuthorize)
        {
#if TSOPreAlpha
            CSR = false;
#endif
            if (CSR)
                RootElement.Add(new XElement(TSOCitySelectorUserCSRAuthorize));
        }
    }
}

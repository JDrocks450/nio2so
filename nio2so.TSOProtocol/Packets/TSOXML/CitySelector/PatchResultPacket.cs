using nio2so.Protocol.Data.Credential;
using System.Xml.Linq;

namespace nio2so.TSOProtocol.Packets.TSOXML.CitySelector
{
    /// <summary>
    /// Used when the TSOClient connecting to the CitySelector appears to be outdated. 
    /// <para>It will poke the user to apply an update; if they accept, our buddy(.exe) will be invoked.</para>
    /// </summary>
    public class PatchResultPacket : TSOXMLPacket
    {
        public const string TSOCitySelectorPatchResult = "Patch-Result";
        public const string TSOCitySelectorPatchResultTicket = "Authorization-Ticket";
        public const string TSOCitySelectorPatchResultAddress = "Patch-Address";

        /// <summary>
        /// Makes a new <see cref="UserAuthorizePacket"/> optionally with the user being a CSR.
        /// <para>TSO PreAlpha is not compatible with this and nio2so will automatically disable it. </para>
        /// </summary>
        /// <param name="CSR"></param>
        public PatchResultPacket(Uri PatchAddress, TSOSessionTicket Ticket) : base(TSOCitySelectorPatchResult)
        {
            RootElement.Add(
                new XElement(TSOCitySelectorPatchResultTicket, Ticket.ToString()),
                new XElement(TSOCitySelectorPatchResultAddress, PatchAddress.ToString())
            );
        }
    }
}

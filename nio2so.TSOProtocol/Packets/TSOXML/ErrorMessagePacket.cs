using System.Xml.Linq;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML
{
    /// <summary>
    /// An error packet sent by the CitySelector.
    /// <para>Use this for any number of error scenarios.</para>
    /// </summary>
    public class ErrorMessagePacket : TSOXMLPacket
    {
        public const string TSOCitySelectorErrorMessage = "Error-Message";
        public const string TSOCitySelectorErrorNumber = "Error-Number";
        public const string TSOCitySelectorErrorText = "Error";

        /// <summary>
        /// Makes a new <see cref="ErrorMessagePacket"/> optionally with the user being a CSR.
        /// <para>TSO PreAlpha is not compatible with this and nio2so will automatically disable it. </para>
        /// </summary>
        /// <param name="CSR"></param>
        public ErrorMessagePacket(int ErrorCode, string Error) : base(TSOCitySelectorErrorMessage)
        {
            RootElement.Add(
                new XElement(TSOCitySelectorErrorNumber, ErrorCode.ToString()),
                new XElement(TSOCitySelectorErrorText, Error)
            );
        }
    }
}

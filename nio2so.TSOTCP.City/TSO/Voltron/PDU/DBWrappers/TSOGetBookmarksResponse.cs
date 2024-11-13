using nio2so.TSOTCP.City.TSO.Voltron.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{    
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetBookmarksQuery"/>
    /// </summary>
    internal class TSOGetBookmarksResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetBookmarksResponse(string AriesID, string MasterID, uint TransactionID) :
            base(AriesID,
                MasterID,
                0x0000,
                0x00000041,
                (uint)TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard,
                0x21,
                TransactionID,
                (uint)TSO_PreAlpha_DBActionCLSIDs.GetBookmarksResponse,
                new byte[]
                {
                    // ** Emplace this data **
                    0x00,0x00,0x05,0x3A,
                    0x00,0x00,0x00,0x01,
                    0x00,0x00,0x00,0xA1,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    // ** Then use the API to fill the rest with garbage
                    // ** Please note, see: TSODBWrapperPDU.FillAvailableSpace()
                })
        {

        }
    }
}

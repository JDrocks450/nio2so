using nio2so.TSOTCP.City.TSO.Voltron.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotIDRequest"/>
    /// </summary>
    internal class TSOGetRoommateInfoByLotIDResponse : TSODBRequestWrapper
    {
        public TSOGetRoommateInfoByLotIDResponse(string AriesID, string MasterID) :
            base(AriesID,
                MasterID,
                0x0000,
                0x00000041,
                (uint)TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard,
                0x21,
                0xDBF301A9,
                (uint)TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotIDResponse,
                new byte[]
                {
                    // ** Emplace this data **
                    0x00,0x00,0x05,0x3A,
                    0x00,0x00,0x00,0x01,
                    0x00,0x00,0x05,0x39,
                    // ** Then use the API to fill the rest with garbage
                    // ** Please note, see: TSODBWrapperPDU.FillAvailableSpace()
                })
        {

        }
    }
}

using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator(nameof(SearchProtocol))]
    internal class SearchProtocol : ITSOProtocolRegulator
    {
        public string RegulatorName => nameof(SearchProtocol);

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            Response = null;

            switch ((TSO_PreAlpha_DBStructCLSIDs)PDU.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage:
                    switch ((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID)
                    {
                        case TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Request:
                            { // Exact search
                                TSOExactSearchRequest searchPDU = (TSOExactSearchRequest)PDU;
                                string searchTerm = searchPDU.SearchTerm;
                                TSO_PreAlpha_SearchCategories category = searchPDU.SearchResourceType;

                                Response = new(new[] { new TSOExactSearchResponse(category, TSOVoltronConst.MyAvatarID) }, null, null);
                            }
                            return true;
                    }
                    break;
            }

            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            Response = null;

            //There are no PDUs related to search.

            return false; 
        }
    }
}

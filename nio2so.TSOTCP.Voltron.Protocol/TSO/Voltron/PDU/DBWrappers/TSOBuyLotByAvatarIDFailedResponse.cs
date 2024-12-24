using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View
    /// <para/>
    /// </summary>
    // You cannot map two classes to one type. [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response)]
    internal class TSOBuyLotByAvatarIDFailedResponse : TSODBRequestWrapper
    {          
        public TSOBuyLotByAvatarIDFailedResponse(uint ErrorCode) : 
            base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Request
            ) 
        {
            MakeBodyFromProperties();
        }
    }
}

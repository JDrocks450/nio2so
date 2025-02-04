﻿namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View
    /// <para/>
    /// </summary>
    // You cannot map two classes to one type. [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response)]
    public class TSOBuyLotByAvatarIDFailedResponse : TSODBRequestWrapper
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

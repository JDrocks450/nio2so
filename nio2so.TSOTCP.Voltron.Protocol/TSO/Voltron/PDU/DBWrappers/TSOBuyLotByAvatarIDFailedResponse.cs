namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View, and it cannot be completed. 
    /// <para/> This is a <see cref="TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response"/> which is ordinarily mapped to <see cref="TSOBuyLotByAvatarIDResponse"/>.
    /// This is a <see cref="TSOVoltronPacket"/> that is not mapped using the <see cref="TSOVoltronPDU"/> attribute because it exists for code clarity.
    /// </summary>    
    public sealed class TSOBuyLotByAvatarIDFailedResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The NewHouse ID is 0x0 for a failed house buy transaction
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint ErrorCode { get; set; } = 0x00;
        /// <summary>
        /// Creates a new <see cref="TSOBuyLotByAvatarIDFailedResponse"/> packet that will prompt the game client to report that buying the house failed
        /// </summary>
        public TSOBuyLotByAvatarIDFailedResponse() :
            base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response
            )
        {                        
            MakeBodyFromProperties();
        }
    }
}

using nio2so.Formats.DB;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View
    /// <para/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response)]
    public class TSOBuyLotByAvatarIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The ID in the database of the newly created house
        /// </summary>
        [TSOVoltronDBWrapperField] public uint NewHouseID { get; set; }
        /// <summary>
        /// The funds our Avatar is left with after buying this house
        /// </summary>
        [TSOVoltronDBWrapperField] public uint NewFunds { get; set; }
        /// <summary>
        /// The X location of the house
        /// </summary>
        [TSOVoltronDBWrapperField] public TSODBLotPosition LotPosition { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler4 { get; set; } = 0x0E0F;
        [TSOVoltronDBWrapperField] public uint Filler5 { get; set; } = 0x1011;
        /// <summary>
        /// Thumbnail?
        /// </summary>
        [TSOVoltronDBWrapperField] public uint RemainingBytes { get; set; } = 0x0;

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOBuyLotByAvatarIDResponse() : base() { }

        /// <summary>
        /// Confirms to the Client that buying this house was successful
        /// </summary>
        /// <param name="NewHouseID"></param>
        /// <param name="Funds"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public TSOBuyLotByAvatarIDResponse(uint NewHouseID, uint Funds, TSODBLotPosition Position) :
            base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response
            )
        {
            this.NewHouseID = NewHouseID;
            NewFunds = Funds;
            LotPosition = Position;

            MakeBodyFromProperties();
        }
    }
}

using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View
    /// <para/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response)]
    internal class TSOBuyLotByAvatarIDResponse : TSODBRequestWrapper
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
        [TSOVoltronDBWrapperField] public uint Lot_X { get; set; }
        /// <summary>
        /// The Y location of the house
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_Y { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler4 { get; set; } = 0x0E0F;
        [TSOVoltronDBWrapperField] public uint Filler5 { get; set; } = 0x1011;
        /// <summary>
        /// Thumbnail?
        /// </summary>
        [TSOVoltronDBWrapperField] public uint RemainingBytes { get; set; } = 0x0;

        /// <summary>
        /// Confirms to the Client that buying this house was successful
        /// </summary>
        /// <param name="NewHouseID"></param>
        /// <param name="Funds"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public TSOBuyLotByAvatarIDResponse(uint NewHouseID, uint Funds, uint X, uint Y) :
            base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Response
            )
        {
            this.NewHouseID = NewHouseID;
            NewFunds = Funds;
            Lot_X = X; Lot_Y = Y;

            MakeBodyFromProperties();
        }
    }
}

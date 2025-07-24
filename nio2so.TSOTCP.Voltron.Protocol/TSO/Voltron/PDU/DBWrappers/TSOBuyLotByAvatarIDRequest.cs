using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.DB;
using nio2so.DataService.Common.Types.Lot;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Request)]
    public class TSOBuyLotByAvatarIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The Index of the cell in the World Grid
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string LotPhoneNumber { get; set; }
        /// <summary>
        /// The purchaser of the Lot
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// The X location of the Lot on the grid
        /// </summary>
        [TSOVoltronDBWrapperField] public LotPosition LotPosition { get; set; }    

        public TSOBuyLotByAvatarIDRequest() : base() { }
    }
}

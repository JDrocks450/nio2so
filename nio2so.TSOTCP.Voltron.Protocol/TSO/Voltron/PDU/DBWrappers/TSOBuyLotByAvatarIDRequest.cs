using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Player requests to purchase a lot in the World View
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.BuyLotByAvatarID_Request)]
    internal class TSOBuyLotByAvatarIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The Index of the cell in the World Grid
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string LotIndexString { get; set; }
        /// <summary>
        /// The purchaser of the Lot
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// The X location of the Lot on the grid
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_X { get; set; }
        /// <summary>
        /// The Y location of the Lot on the grid
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_Y { get; set; }

        public TSOBuyLotByAvatarIDRequest() : base() { }
    }
}

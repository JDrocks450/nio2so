using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// This will return <i>stubs</i> of Lots in the City, the stubs contain basic information about
    /// a lot, which will then be harvested from the server using the <see cref="TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request"/>
    /// PDU to get additional information about the house.
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetLotList_Response)]
    internal class TSOGetLotListResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; } = 0x10111213;
        [TSOVoltronDBWrapperField] public uint LotCount { get; set; } = 0x01;

        // ** LotList_Item Schema follows
        /// <summary>
        /// This is the size of the item in the list, in bytes.
        /// Do not go over this length of bytes on a per-item basis.
        /// <para/>Therefore, your byte length should be exactly:
        /// <c>LotCount * 0x1C</c>
        /// </summary>
        const uint SCHEMA_ITEMSIZE = 0x1C;
        //**Schema fields begin
        /// <summary>
        /// The ID of the Lot in the Database 
        /// <para/><i>(LotID -- or in logs: lotUD -- likely a typo)</i>
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_DatabaseID { get; set; }
        /// <summary>
        /// The X component of the lot's Location
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_X { get; set; }
        /// <summary>
        /// The Y component of the lot's Location
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_Y { get; set; }
        /// <summary>
        /// <i>Unknown</i>
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Unknown1 { get; set; } = 0x01;
        /// <summary>
        /// <i>Unknown</i>
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Unknown2 { get; set; } = 0x01;
        /// <summary>
        /// subject to change
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Thumbnail { get; set; } = 0x00;
        /// <summary>
        /// subject to change
        /// </summary>
        [TSOVoltronDBWrapperField] public uint BuildSize { get; set; } = 0xFF;
        //**Schema end
        /// <summary>
        /// It is unknown what this does, but it needs to be here for the game to 
        /// accept the incoming information
        /// </summary>
        [TSOVoltronDBWrapperField] public byte[] FooterData => new byte[4 * 8].TSOFillArray();


        public TSOGetLotListResponse(uint LotID, uint X, uint Y) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetLotList_Response
            )
        {
            Lot_DatabaseID = LotID;
            Lot_X = X; Lot_Y = Y;

            MakeBodyFromProperties();
        }
    }
}

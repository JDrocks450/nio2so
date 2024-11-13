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
        /// <summary>
        /// A byte array for the body of the DBRequestWrapper that seems to work?
        /// </summary>
        private byte[] DBBody_research = new byte[]
        {
            // ** Emplace this data **
            0x00,0x00,0x05,0x3A, //<---HOUSEID
            0x00,0x00,0x00,0x01,
            0x00,0x00,0x00,0xA1
            // ** Then use the API to fill the rest with garbage
            // ** Please note, see: TSODBWrapperPDU.FillAvailableSpace()
        };

        /// <summary>
        /// The ID of the House we're getting information on
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public uint Value1 { get; set; } = 0x01;
        [TSOVoltronDBWrapperField] public uint OwnerAvatarID { get; set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetRoommateInfoByLotIDResponse(string AriesID, string MasterID, uint TransactionID, uint HouseID, uint ownerAvatarID) :
            base(AriesID,
                MasterID,
                0x0000,
                0x82, //TSODBWrapperMessageSize.AutoSize,
                (uint)TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard,
                0x21,
                TransactionID,
                (uint)TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotIDResponse,
                new byte[12])
        {
            this.HouseID = HouseID;
            OwnerAvatarID = ownerAvatarID;

            FillPacketToAvailableSpace();
            MoveBufferPositionToDBMessageBody();
            EmplaceBody(HouseID);
            EmplaceBody(Value1);
            EmplaceBody(OwnerAvatarID);
            
        }
    }
}

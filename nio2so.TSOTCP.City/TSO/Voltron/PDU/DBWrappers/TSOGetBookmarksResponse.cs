using nio2so.TSOTCP.City.TSO.Voltron.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{    
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetBookmarksQuery"/>
    /// </summary>
    internal class TSOGetBookmarksResponse : TSODBRequestWrapper
    {
        //use this in case you forget the formatting
        byte[] reference_material = new byte[]
        {
#region DATA
                    // ** Emplace this data **
                    0x00,0x00,0x05,0x30, // <-- My AvatarID? will not proceed unless matches request
                    0x00,0x00,0x00,0x01, // <-- type? not sure what this is yet
                    0x00,0x00,0x00,0x03, // <-- Amount of items in bookmarks
                    0x00,0x00,0x00,0x24,
                    0x00,0x00,0x00,0x25,
                    0x00,0x00,0x00,0x26,
                    0x00,0x01,0x00,0x24, // <-- in this example will stop before this item since amount of items
                    0x00,0x02,0x00,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    // ** Then use the API to fill the rest with garbage
                    // ** Please note, see: TSODBWrapperPDU.FillAvailableSpace()                
#endregion
        };

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetBookmarksResponse(string AriesID, string MasterID, uint AvatarID, uint Type, params uint[] ItemIDs) :
            base(AriesID,
                MasterID,
                0x0000,
                DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE + (uint)(12 + (ItemIDs.Length * sizeof(uint))),
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                0x21,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response,
                new byte[] { })
        {
            MoveBufferPositionToDBMessageBody();
            EmplaceBody(AvatarID);              // AvatarID
            EmplaceBody((uint)0x01);                  // Type?
            EmplaceBody((uint)ItemIDs.Length);  // Num Items
            foreach (var id in ItemIDs)
                EmplaceBody(id); // bookmark item ID
            ReadAdditionalMetadata();
        }
    }
}

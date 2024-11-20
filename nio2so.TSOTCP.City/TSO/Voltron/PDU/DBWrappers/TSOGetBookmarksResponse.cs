using nio2so.Formats.Util.Endian;
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
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response)] 
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

        [TSOVoltronDBWrapperField] public uint AvatarID { get; }
        [TSOVoltronDBWrapperField] public uint ListType { get; }
        [TSOVoltronDBWrapperField] public uint ItemCount { get; }
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray] public byte[] ItemList { get; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetBookmarksResponse(uint AvatarID, uint ListType, params uint[] ItemIDs) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response
                )
        {
            this.AvatarID = AvatarID;
            this.ListType = ListType;
            ItemCount = (uint)ItemIDs.Length;
            ItemList = new byte[sizeof(uint) * ItemCount];
            int index = -1;
            foreach (uint ItemID in ItemIDs)
            {
                index++;
                byte[] lotIdBytes = EndianBitConverter.Big.GetBytes(ItemID);
                ItemList[index * sizeof(uint)] = lotIdBytes[0];
                ItemList[index * sizeof(uint) + 1] = lotIdBytes[1];
                ItemList[index * sizeof(uint) + 2] = lotIdBytes[2];
                ItemList[index * sizeof(uint) + 3] = lotIdBytes[3];
            }
            MakeBodyFromProperties();
        }        
    }
}

using nio2so.Formats.Util.Endian;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetBookmarksQuery"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response)] 
    internal class TSOGetBookmarksResponse : TSODBRequestWrapper
    {
        // ! NOTES ABOUT THIS PDU !
        // ListType is only ever 0x01
        // Item count is confirmed to be the correct value
        // All Char DBIds in the list will be sorted in the Bookmarks list by their AvatarID -- not the order they were added in
        // The AvatarID that belongs to the current avatar downloading the bookmarks can match a bookmark in the list
        // (as in -- you can bookmark yourself without issue

        [TSOVoltronDBWrapperField] public uint AvatarID { get; }
        [TSOVoltronDBWrapperField] public uint ListType { get; }
        [TSOVoltronDBWrapperField] public uint ItemCount { get; }
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] ItemList { get; }        

        /// <summary>
        /// Creates a new <see cref="TSOGetBookmarksResponse"/> PDU with the provided parameters
        /// </summary>
        /// <param name="AvatarID">The AvatarID we're supplying bookmarks for</param>
        /// <param name="ListType">Not functional in this version? Always gets added to Avatar List</param>
        /// <param name="ItemIDs">The IDs of the items to add to the Bookmarks list</param>
        public TSOGetBookmarksResponse(uint AvatarID, TSO_PreAlpha_SearchCategories ListType, params uint[] ItemIDs) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response
                )
        {
            this.AvatarID = AvatarID;
            this.ListType = (uint)ListType;
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

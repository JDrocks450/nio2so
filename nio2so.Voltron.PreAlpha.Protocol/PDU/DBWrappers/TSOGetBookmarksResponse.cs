using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetBookmarksQuery"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response)]
    public class TSOGetBookmarksResponse : TSODBRequestWrapper
    {
        // ! NOTES ABOUT THIS PDU !
        // ListType is only ever 0x01
        // Item count is confirmed to be the correct value
        // All Char DBIds in the list will be sorted in the Bookmarks list by their AvatarID -- not the order they were added in
        // The AvatarID that belongs to the current avatar downloading the bookmarks can match a bookmark in the list
        // (as in -- you can bookmark yourself without issue

        /// <summary>
        /// Represents a bookmark
        /// </summary>
        public record TSOBookmark
        {
            public TSOBookmark()
            {
            }

            public TSOBookmark(uint avatarID)
            {
                AvatarID = avatarID;
            }

            public uint AvatarID { get; set; }
        }

        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public TSO_PreAlpha_Categories ListType { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronArrayLength(nameof(Bookmarks))] public uint BookmarkCount { get; set; }
        [TSOVoltronDBWrapperField] public TSOBookmark[] Bookmarks { get; set; } = new TSOBookmark[0];

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetBookmarksResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOGetBookmarksResponse"/> PDU with the provided parameters
        /// </summary>
        /// <param name="AvatarID">The AvatarID we're supplying bookmarks for</param>
        /// <param name="ListType">Not functional in this version? Always gets added to Avatar List</param>
        /// <param name="ItemIDs">The IDs of the items to add to the Bookmarks list</param>
        public TSOGetBookmarksResponse(uint AvatarID, params uint[] ItemIDs) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Response
                )
        {
            this.AvatarID = AvatarID;
            ListType = TSO_PreAlpha_Categories.Avatar;
            Bookmarks = ItemIDs.Select(x => new TSOBookmark(x)).ToArray();
            MakeBodyFromProperties();
        }
    }
}

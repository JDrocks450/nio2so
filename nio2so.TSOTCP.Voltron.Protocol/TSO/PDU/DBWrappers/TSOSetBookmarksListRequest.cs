using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    /// <summary>
    /// Matches the structure of a <see cref="TSOGetBookmarksResponse"/>. Sent when the Client changes their bookmarks
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetBookmarks_Request)]
    public class TSOSetBookmarksListRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The avatarID of the avatar we're setting bookmarks list for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public TSO_PreAlpha_Categories BookmarksListType { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronArrayLength(nameof(Bookmarks))] public uint BookmarksCount { get; set; }
        [TSOVoltronDBWrapperField] public uint[] Bookmarks { get; set; }  = new uint[0]; 

        public TSOSetBookmarksListRequest() : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
            TSO_PreAlpha_DBActionCLSIDs.SetBookmarks_Request
            )
        {
            MakeBodyFromProperties();
        }        
    }
}

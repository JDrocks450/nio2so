namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Request)]
    public class TSOGetBookmarksRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Needs further testing to confirm
        /// </summary>
        [TSOVoltronDBWrapperField] public uint ListType { get; set; }

        public TSOGetBookmarksRequest() : base()
        {

        }
    }
}

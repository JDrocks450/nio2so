namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Request)]
    public class TSOGetBookmarksRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        
        [TSOVoltronDBWrapperField] public TSO_PreAlpha_Categories ListType { get; set; }

        public TSOGetBookmarksRequest() : base()
        {

        }
    }
}

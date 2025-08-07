namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Request)]
    public class TSOGetCharBlobByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }

        public TSOGetCharBlobByIDRequest() : base()
        {

        }
    }
}

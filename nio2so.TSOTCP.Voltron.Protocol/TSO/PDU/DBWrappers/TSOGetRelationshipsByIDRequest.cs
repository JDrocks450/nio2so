namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Request)]
    public class TSOGetRelationshipsByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }

        public TSOGetRelationshipsByIDRequest() : base() { }
    }
}

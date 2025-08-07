namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetReversedRelationshipsByID_Request)]
    public class TSOGetReversedRelationshipsByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }

        public TSOGetReversedRelationshipsByIDRequest() : base() {  }
    }
}

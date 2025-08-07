namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.DeleteRelationship_Request)]
    public class TSODeleteRelationshipRequest : TSODBRequestWrapper
    {
        public TSODeleteRelationshipRequest() : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kSimRequestMsg,
            TSO_PreAlpha_DBActionCLSIDs.DeleteRelationship_Request
        )
        {
        }

        [TSOVoltronDBWrapperField] uint ReceiverAvatarID { get; set; }
        [TSOVoltronDBWrapperField] uint SenderAvatarID { get; set; }
    }
}

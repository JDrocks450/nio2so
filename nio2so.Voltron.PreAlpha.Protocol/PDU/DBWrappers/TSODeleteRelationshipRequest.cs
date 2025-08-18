namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.DeleteRelationship_Request)]
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

using nio2so.DataService.Common.Types.Avatar;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// This structure is sent to a Client that returns a list of how other sims feel about the <b>Target Avatar</b>
    /// Structure works
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetReversedRelationshipsByID_Response)]
    public class TSOGetReversedRelationshipsByIDResponse : TSODBRequestWrapper
    { // HOW THEY FEEL
        /// <summary>
        /// The Avatar that is being asked about relationships for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint StatusCode { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronArrayLength(nameof(Relationships))] public uint RelationshipsCount { get; set; }
        /// <summary>
        /// The sender field for each <see cref="AvatarRelationship"/> must match the target avatar <see cref="AvatarID"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public AvatarRelationship[] Relationships { get; set; } = Array.Empty<AvatarRelationship>();

        /// <summary>
        /// <inheritdoc cref="TSORelationshipResponsePDUBase(TSO_PreAlpha_DBStructCLSIDs, TSO_PreAlpha_kMSGs, TSO_PreAlpha_DBActionCLSIDs)"/>
        /// </summary>
        public TSOGetReversedRelationshipsByIDResponse() : base(TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage, TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBActionCLSIDs.GetReversedRelationshipsByID_Response)
        { }

        /// <summary>
        /// Creates a new <see cref="TSORelationshipResponsePDUBase"/> PDU
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="RelationshipList"></param>
        public TSOGetReversedRelationshipsByIDResponse(uint AvatarID, params AvatarRelationship[] RelationshipsList) : this()
        {
            this.AvatarID = AvatarID;
            StatusCode = 0; // no error
            Relationships = RelationshipsList;

            MakeBodyFromProperties();
        }

        public static TSOGetReversedRelationshipsByIDResponse GetErrorResponse(uint AvatarID, uint StatusCode)
        {
            var pdu = new TSOGetReversedRelationshipsByIDResponse(AvatarID) { StatusCode = StatusCode };
            pdu.MakeBodyFromProperties();
            return pdu;
        }
    }
}

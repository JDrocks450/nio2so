using nio2so.DataService.Common.Types.Avatar;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{    
    /// <summary>
    /// This structure is sent to a Client that returns a list of how the <b>Target Avatar</b> feels about other sims
    /// Structure works
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response)]
    public class TSOGetRelationshipsByIDResponse : TSODBRequestWrapper
    { // HOW I FEEL
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
        /// <inheritdoc cref="TSODBRequestWrapper(TSO_PreAlpha_DBStructCLSIDs, TSO_PreAlpha_kMSGs, TSO_PreAlpha_DBActionCLSIDs)"/>
        /// </summary>
        public TSOGetRelationshipsByIDResponse() : base(TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage, TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response)
        { }

        /// <summary>
        /// Creates a new <see cref="TSOGetRelationshipsByIDResponse"/> PDU
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="RelationshipsList"></param>
        public TSOGetRelationshipsByIDResponse(uint AvatarID, params AvatarRelationship[] RelationshipsList) : this()
        {
            this.AvatarID = AvatarID;
            StatusCode = 0; // no error
            Relationships = RelationshipsList;

            MakeBodyFromProperties();
        }

        public static TSOGetRelationshipsByIDResponse GetErrorResponse(uint AvatarID, uint StatusCode)
        {
            var pdu = new TSOGetRelationshipsByIDResponse(AvatarID) { StatusCode = StatusCode };
            pdu.MakeBodyFromProperties();
            return pdu;
        }
    }
}

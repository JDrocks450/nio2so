namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Structure works
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetReversedRelationshipsByID_Response)]
    public class TSOGetReversedRelationshipsByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The Avatar that is being asked about relationships for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint StatusCode { get; set; }
        [TSOVoltronDBWrapperField]
        public TSOGetRelationshipsByIDResponse.TSORelationshipStructure Test { get; set; } = new()
        {
            Param1 = 1,            
            FirstLevel = 0,
            SecondLevel = 161,
            ThirdLevel = 1337,
            Param2 = 1,
        };

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetReversedRelationshipsByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOGetRelationshipsByIDResponse"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="FriendAvatarIDs"></param>
        public TSOGetReversedRelationshipsByIDResponse(uint AvatarID, params uint[] FriendAvatarIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetReversedRelationshipsByID_Response
            )
        {
            this.AvatarID = AvatarID;
            MakeBodyFromProperties();
        }
    }
}

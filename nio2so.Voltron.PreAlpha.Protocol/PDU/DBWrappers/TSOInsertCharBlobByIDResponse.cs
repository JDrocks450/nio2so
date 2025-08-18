namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// Sends a confirmation to the Client that the <c>TSODBCharBlob</c> was received successfully
    /// </summary>
    public class TSOInsertCharBlobByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField]
        public uint NewAvatarID { get; set; }
        [TSOVoltronDBWrapperField]
        public uint StatusCode { get; set; } = 0x01;

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOInsertCharBlobByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOInsertCharBlobByIDResponse"/>
        /// </summary>
        /// <param name="NewAvatarID"></param>
        public TSOInsertCharBlobByIDResponse(uint NewAvatarID) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Response
                )
        {
            this.NewAvatarID = NewAvatarID;
            MakeBodyFromProperties();
        }
    }
}

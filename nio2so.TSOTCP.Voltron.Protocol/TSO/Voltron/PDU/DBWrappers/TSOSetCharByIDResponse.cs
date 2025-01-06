namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sends a confirmation to the Client that the character data was received successfully
    /// </summary>
    internal class TSOSetCharByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0x01;

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOSetCharByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOSetCharByIDResponse"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        public TSOSetCharByIDResponse(uint AvatarID) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Response
                )
        {
            this.AvatarID = AvatarID;
            MakeBodyFromProperties();
        }        
    }
}

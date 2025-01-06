namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Confirms with the Client what the money fields for the Avatar should be.
    /// </summary>   
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetMoneyFields_Response)]
    public class TSOSetMoneyFieldsResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg2 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg3 { get; set; }

        public TSOSetMoneyFieldsResponse() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.SetMoneyFields_Response
            ) { }        

        /// <summary>
        /// Creates a new <see cref="TSOSetMoneyFieldsResponse"/>
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public TSOSetMoneyFieldsResponse(uint avatarID, uint arg1 = 0, uint arg2 = 0, uint arg3 = 0) : this()
        {
            AvatarID = avatarID;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            MakeBodyFromProperties();
        }
    }
}

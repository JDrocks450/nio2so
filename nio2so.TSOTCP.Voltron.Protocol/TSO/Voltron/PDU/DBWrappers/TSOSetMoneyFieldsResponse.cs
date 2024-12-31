namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Confirms with the Client what the money fields for the Avatar should be.
    /// </summary>   
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetMoneyFields_Response)]
    internal class TSOSetMoneyFieldsResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg2 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg3 { get; set; }

        public TSOSetMoneyFieldsResponse() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.SetMoneyFields_Response
            )
        {

        }

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

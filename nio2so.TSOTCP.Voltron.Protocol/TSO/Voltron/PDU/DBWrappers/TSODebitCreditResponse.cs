namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent from the remote party when a charge appears on the avatar's funds
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Response)]
    public class TSODebitCreditResponsePDU : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Untested -- placeholder label name
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Account { get; set; }
        /// <summary>
        /// Untested -- placeholder label name
        /// </summary>
        [TSOVoltronDBWrapperField] public int Amount { get; set; }

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSODebitCreditResponsePDU() : base() { }
        public TSODebitCreditResponsePDU(uint avatarID, uint account, int amount) : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Response
            )
        {
            AvatarID = avatarID;
            Account = account;
            Amount = amount;

            MakeBodyFromProperties();
        }
    }
}

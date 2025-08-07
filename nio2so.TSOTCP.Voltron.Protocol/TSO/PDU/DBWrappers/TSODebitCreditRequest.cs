namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Request)]
    public class TSODebitCreditRequestPDU : TSODBRequestWrapper
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

        public TSODebitCreditRequestPDU() : base() { }
    }
}

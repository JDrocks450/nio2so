namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Request)]
    internal class TSODebitCreditRequestPDU : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Untested -- placeholder label name
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Account { get; set; }
        /// <summary>
        /// Untested -- placeholder label name
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Amount { get; set; }

        public TSODebitCreditRequestPDU() : base() { }
    }
}

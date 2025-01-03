namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// This is sent after closing Edit A Sim and after the SetCharByID_Request PDU.
    /// <para/>This seems to set the money fields of the Avatar (Total Money, Passive Money, etc.)
    /// <para/> Seems very dangerous for the Client to be setting the money field -- especially since Money
    /// shouldn't change in CAS.
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetMoneyFields_Request)]
    internal class TSOSetMoneyFieldsRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The ID of the Avatar
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Money? Probably. Needs testing
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg2 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg3 { get; set; }

        public TSOSetMoneyFieldsRequest()
        {
        }
    }
}

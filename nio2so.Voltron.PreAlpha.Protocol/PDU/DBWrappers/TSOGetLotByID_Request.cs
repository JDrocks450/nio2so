namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// This PDU will ask the remote connection for information on a lot by the given <c>HouseID</c>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request)]
    public class TSOGetLotByID_Request : TSODBRequestWrapper
    {
        /// <summary>
        /// The ID of the Lot we are requesting information on
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetLotByID_Request() : base() { }
    }
}

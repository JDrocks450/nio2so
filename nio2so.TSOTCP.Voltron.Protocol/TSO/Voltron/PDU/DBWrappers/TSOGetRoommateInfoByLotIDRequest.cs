namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request)]
    internal class TSOGetRoommateInfoByLotIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The ID of the House we're getting information on
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetRoommateInfoByLotIDRequest() :
            base()
        {

        }
    }
}

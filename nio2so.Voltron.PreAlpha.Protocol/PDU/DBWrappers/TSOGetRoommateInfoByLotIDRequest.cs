namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request)]
    public class TSOGetRoommateInfoByLotIDRequest : TSODBRequestWrapper
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

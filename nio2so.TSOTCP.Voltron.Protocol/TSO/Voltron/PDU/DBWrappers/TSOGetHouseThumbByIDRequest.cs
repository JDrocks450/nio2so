namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Client requests the thumbnail after a successful <see cref="TSOGetLotByID_Response"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseThumbByID_Request)]
    public class TSOGetHouseThumbByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetHouseThumbByIDRequest() : base() { }
    }
}

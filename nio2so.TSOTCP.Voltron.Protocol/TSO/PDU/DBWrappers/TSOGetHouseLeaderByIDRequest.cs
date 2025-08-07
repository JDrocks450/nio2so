namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
    public class TSOGetHouseLeaderByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetHouseLeaderByIDRequest() : base() { }
    }
}

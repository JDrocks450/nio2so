namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
    public class TSOGetHouseLeaderByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetHouseLeaderByIDRequest() : base() { }
    }
}

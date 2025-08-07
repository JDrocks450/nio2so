namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request)]
    public class TSOGetHouseBlobByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDRequest() : base() { }

        public TSOGetHouseBlobByIDRequest(uint HouseID) :
            base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request
                )
        {
            this.HouseID = HouseID;
            MakeBodyFromProperties();
        }
    }
}

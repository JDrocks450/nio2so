namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Nothing is known about this PDU... this is a placeholder file
    /// </summary>    
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Response)]
    public class TSOGetHouseLeaderByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public uint LeaderID { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0x1;

        public TSOGetHouseLeaderByIDResponse(uint HouseID, uint LeaderID) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Response
                )
        {
            this.HouseID = HouseID;
            this.LeaderID = LeaderID;

            MakeBodyFromProperties();
        }
    }
}

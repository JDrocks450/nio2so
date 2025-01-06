namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Response packet to <see cref="TSO_PreAlpha_MasterConstantsTable.GZCLSID_cDBGetHouseLeaderByLotID_Request"/>
    /// that indicates to the remote party what the 'leader' of this House is. The Owner basically, could also indicate
    /// who the host of the simulation room is.
    /// </summary>    
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Response)]
    public class TSOGetHouseLeaderByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public uint LeaderID { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0x1;

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetHouseLeaderByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOGetHouseLeaderByIDResponse"/> PDU with the 
        /// <paramref name="HouseID"/> and <paramref name="LeaderID"/> provided
        /// </summary>
        /// <param name="HouseID">The DB HouseID of the House we're in reference to</param>
        /// <param name="LeaderID">The Leader of this house</param>
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

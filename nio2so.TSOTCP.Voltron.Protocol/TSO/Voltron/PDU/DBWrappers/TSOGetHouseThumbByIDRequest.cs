﻿namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Client requests the thumbnail after a successful <see cref="TSOGetLotByID_Response"/> or Zooming into a Region with a lot in it
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseThumbByID_Request)]
    public class TSOGetHouseThumbByIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The requested House's database ID
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetHouseThumbByIDRequest() : base() { }
        public TSOGetHouseThumbByIDRequest(uint HouseID) : this()
        {
            this.HouseID = HouseID;
            MakeBodyFromProperties();
        }
    }
}

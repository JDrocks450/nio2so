﻿namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// This PDU will ask the remote connection for information on a lot by the given <c>HouseID</c>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Request)]
    internal class TSOGetLotByID_Request : TSODBRequestWrapper
    {
        /// <summary>
        /// The ID of the Lot we wish to get information on
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Lot_DatabaseID { get; set; }

        public TSOGetLotByID_Request() : base() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Client requests the thumbnail after a successful <see cref="TSOGetLotByID_Response"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseThumbByID_Request)]
    internal class TSOGetHouseThumbByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        public TSOGetHouseThumbByIDRequest() : base() { }
    }
}

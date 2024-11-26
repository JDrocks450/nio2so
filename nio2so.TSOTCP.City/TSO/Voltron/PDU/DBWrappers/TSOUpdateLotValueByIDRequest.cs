using MiscUtil.Conversion;
using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.UpdateLotValueByID_Request)]
    internal class TSOUpdateLotValueByIDRequest : TSODBRequestWrapper
    {        
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public uint Parameter1 { get; set; }
        [TSOVoltronDBWrapperField] public uint Parameter2 { get; set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOUpdateLotValueByIDRequest() : base() { }    
    }
}

using MiscUtil.Conversion;
using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request)]
    internal class TSOGetHouseBlobByIDRequest : TSODBRequestWrapper
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Request)]
    internal class TSODebitCreditRequestPDU : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Untested -- placeholder label name
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Account { get; set; }
        /// <summary>
        /// Untested -- placeholder label name
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Amount { get; set; }

        public TSODebitCreditRequestPDU() : base() { }    
    }
}

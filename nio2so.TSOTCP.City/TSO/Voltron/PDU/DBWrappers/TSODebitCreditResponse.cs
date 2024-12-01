using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Response)]
    internal class TSODebitCreditResponsePDU : TSODBRequestWrapper
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

        public TSODebitCreditResponsePDU(uint avatarID, uint account, uint amount) : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBActionCLSIDs.DebitCredit_Response
            )
        {
            AvatarID = avatarID;
            Account = account;
            Amount = amount;

            MakeBodyFromProperties();
        }
    }
}

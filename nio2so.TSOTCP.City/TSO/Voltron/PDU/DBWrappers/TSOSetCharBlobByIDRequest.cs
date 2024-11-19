using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Request)]
    internal class TSOSetCharBlobByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint CharBlobSize { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] CharBlobStream { get; set; }

        public TSOSetCharBlobByIDRequest() : base() { }
    }
}

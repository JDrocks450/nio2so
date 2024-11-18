using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request)]
    internal class TSOInsertCharBlobByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint StatusCode { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronString(TSOVoltronValueTypes.SlimPascal)] public string AvatarName { get; set; } = "NotSet";
        [TSOVoltronDBWrapperField] public uint CharBlobSize { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] CharBlobStream { get; set; } = new byte[0];

        public TSOInsertCharBlobByIDRequest() : base()
        {

        }
    }
}

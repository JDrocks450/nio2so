using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Contains a <see cref="TSODBCharBlob"/> object in a cTSOSerializable Stream, which contains a RefPack 
    /// of the char data. 
    /// <para/>RefPack is used to compress data for sending over the airwaves, as well as other usages.
    /// <para/>See: <seealso href="http://wiki.niotso.org/RefPack "/> 
    /// <para/>See also: <seealso href="http://wiki.niotso.org/Stream"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request)]
    internal class TSOInsertCharBlobByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint StatusCode { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string AvatarName { get; set; } = "NotSet";
        [TSOVoltronDBWrapperField] public uint CharBlobSize { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] CharBlobStream { get; set; } = new byte[0];

        public TSOInsertCharBlobByIDRequest() : base()
        {
            
        }
    }
}

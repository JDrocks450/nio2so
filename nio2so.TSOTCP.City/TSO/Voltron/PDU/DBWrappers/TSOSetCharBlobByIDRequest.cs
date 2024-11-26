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
        [TSOVoltronDBWrapperField] public uint RefPackLength { get; set; }
        [TSOVoltronDBWrapperField] public byte CompressionMode { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint DecompressedSize { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint CompressedSize { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint BlobLength { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] CharBlobStream { get; set; } = new byte[0];

        public TSOSetCharBlobByIDRequest() : base() { }
    }
}

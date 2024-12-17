using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.DB;
using nio2so.Formats.Streams;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Request)]
    internal class TSOSetCharBlobByIDRequest : TSODBRequestWrapper, ITSOSerializableStreamPDU1
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint RefPackLength { get; set; }
        [TSOVoltronDBWrapperField] public TSOSerializableStream CharBlobStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU1.GetStream() => CharBlobStream;
        
        public TSOSetCharBlobByIDRequest() : base() { }

        public bool TryUnpack(out TSODBCharBlob? CharBlob) => ((ITSOSerializableStreamPDU1)this).TryUnpackStream(out CharBlob);
    }
}

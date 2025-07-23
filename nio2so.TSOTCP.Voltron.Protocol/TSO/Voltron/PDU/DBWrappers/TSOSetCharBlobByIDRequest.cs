using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats.DB;
using nio2so.Formats.Streams;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Request)]
    public class TSOSetCharBlobByIDRequest : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint RefPackLength { get; set; }
        [TSOVoltronDBWrapperField] public TSOSerializableStream CharBlobStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => CharBlobStream;

        public TSOSetCharBlobByIDRequest() : base() { }

        public bool TryUnpack(out TSODBCharBlob? CharBlob) => ((ITSOSerializableStreamPDU)this).TryUnpackStream(out CharBlob);
    }
}

using nio2so.Data.Common.Serialization.Voltron;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats.Streams;
using nio2so.Voltron.Core.TSO.Serialization.Types;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// Sent when a Client want to update the appearance of their Avatar
    /// <para/>Seen after CAS and when an online house is closing
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Request)]
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

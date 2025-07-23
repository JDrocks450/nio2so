using nio2so.Data.Common.Serialization.Voltron;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats.DB;
using nio2so.Formats.Streams;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Contains a <see cref="TSODBCharBlob"/> object in a cTSOSerializable Stream, which contains a RefPack 
    /// of the char data. 
    /// <para/>RefPack is used to compress data for sending over the airwaves, as well as other usages.
    /// <para/>See: <seealso href="http://wiki.niotso.org/RefPack "/> 
    /// <para/>See also: <seealso href="http://wiki.niotso.org/Stream"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request)]
    public class TSOInsertCharBlobByIDRequest : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint StatusCode { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string AvatarName { get; set; } = "NotSet";
        [TSOVoltronDBWrapperField] public uint RefPackLength { get; set; }
        [TSOVoltronDBWrapperField] public TSOSerializableStream CharBlobStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => CharBlobStream;

        public TSOInsertCharBlobByIDRequest() : base()
        {

        }

        public bool TryUnpack(out TSODBCharBlob? Blob) => ((ITSOSerializableStreamPDU)this).TryUnpackStream(out Blob);
    }
}

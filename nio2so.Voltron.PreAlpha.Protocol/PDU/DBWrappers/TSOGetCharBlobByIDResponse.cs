using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats.Streams;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.Core.TSO.Serialization.Types;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// Contains a <see cref="TSODBCharBlob"/> object in a cTSOSerializable Stream, which contains a RefPack 
    /// of the char data. 
    /// <para/>RefPack is used to compress data for sending over the airwaves, as well as other usages.
    /// <para/>See: <seealso href="http://wiki.niotso.org/RefPack "/> 
    /// <para/>See also: <seealso href="http://wiki.niotso.org/Stream"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response)]
    public class TSOGetCharBlobByIDResponse : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        const uint HEADERLEN = 0xD;

        /// <summary>
        /// The AvatarID in the database that is being received
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint AvatarID { get; set; }
        /// <summary>
        /// Not sure, works with 0x01
        /// </summary>
        [TSOVoltronDBWrapperField]
        public byte Filler { get; set; } = 0x00;
        /// <summary>
        /// The size of the proceeding data from the end of this dword to the end of the payload
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronDistanceToEnd]
        public uint PayloadSize { get; set; }

        //***ITSOSerializableStream

        [TSOVoltronDBWrapperField] public TSOSerializableStream CharBlobStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => CharBlobStream;

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetCharBlobByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOGetCharBlobByIDResponse"/> packet with the provided <paramref name="BlobData"/>
        /// <para/>Please ensure you call <see cref="TSODBCharBlob.EnsureNoErrors"/> before passing this parameter 
        /// to ensure you have the correct format
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="BlobData"></param>
        public TSOGetCharBlobByIDResponse(uint AvatarID, TSODBCharBlob BlobData) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response
                )
        {
            this.AvatarID = AvatarID;

            var decompressedBytes = TSOVoltronSerializer.Serialize(BlobData);
            CharBlobStream = TSOSerializableStream.ToCompressedStream(decompressedBytes);

            MakeBodyFromProperties();
        }
    }
}
using nio2so.Formats.DB;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// This <see cref="TSOVoltronDBRequestWrapperPDU"/> contains a <c>TSOSerializableStream</c>
    /// </summary>
    interface ITSOSerializableStreamPDU
    {
        /// <summary>
        /// 0x00 - None? 0x01 - little endian? 0x03 - Big endian?
        /// <para/>For context, only 0x01 has ever been seen from/to the client in this version
        /// <see cref="TSOSerializableStream"/>
        /// </summary>
        [TSOVoltronDBWrapperField]
        byte CompressionMode { get; set; }
        /// <summary>
        /// The decompressed size of this TSOSerializableStream object
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        uint DecompressedSize { get; set; }
        /// <summary>
        /// The compressed size of the TSOSerializableStream -- basically just the distance from the end of this DWORD to 
        /// the end of the payload
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        uint CompressedSize { get; set; }
        /// <summary>
        /// The size of the proceeding stream including these 4 bytes.
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        uint StreamBytesSize { get; set; }
        /// <summary>
        /// The payload of this <see cref="ITSOSerializableStreamPDU"/>
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        byte[] StreamBytes { get; set; }
    }

    /// <summary>
    /// Contains a <see cref="TSODBCharBlob"/> object in a cTSOSerializable Stream, which contains a RefPack 
    /// of the char data. 
    /// <para/>RefPack is used to compress data for sending over the airwaves, as well as other usages.
    /// <para/>See: <seealso href="http://wiki.niotso.org/RefPack "/> 
    /// <para/>See also: <seealso href="http://wiki.niotso.org/Stream"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response)]
    internal class TSOGetCharBlobByIDResponse : TSODBRequestWrapper, ITSOSerializableStreamPDU
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
        public byte Filler { get; set; } = 0x01;
        /// <summary>
        /// The size of the proceeding data from the end of this dword to the end of the payload
        /// </summary>
        [TSOVoltronDBWrapperField]        
        public uint PayloadSize { get; set; }

        //***ITSOSerializableStream

        /// <summary>
        /// 0x00 - None? 0x01 - little endian? 0x03 - Big endian?
        /// <para/>For context, only 0x01 has ever been seen from/to the client in this version
        /// <see cref="TSOSerializableStream"/>
        /// </summary>
        [TSOVoltronDBWrapperField]  
        public byte CompressionMode { get; set; } = 0x01;
        /// <summary>
        /// The decompressed size of this TSOSerializableStream object
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        public uint DecompressedSize { get; set; }
        /// <summary>
        /// The compressed size of the TSOSerializableStream -- basically just the distance from the end of this DWORD to 
        /// the end of the payload
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        public uint CompressedSize { get; set; }
        /// <summary>
        /// The size of the proceeding stream including these 4 bytes.
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        public uint StreamBytesSize {  get; set; }
        /// <summary>
        /// The RefPack stream containing the CharBlob
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] StreamBytes { get; set; } = new byte[0];

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
            PayloadSize = BlobData.Length + HEADERLEN;
            CompressionMode = 0x01;
            DecompressedSize = BlobData.DecompressedSize;
            CompressedSize = StreamBytesSize = BlobData.Length + sizeof(uint);
            StreamBytes = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }
}
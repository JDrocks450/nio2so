//#define ATTEMPT_1

using nio2so.Formats.DB;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    internal class TSOGetHouseBlobByIDResponseTEST : TSODBRequestWrapper //ITSOSerializableStreamPDU
    {               
#if ATTEMPT_1    
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }    
        [TSOVoltronDBWrapperField] public byte Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint HB_PayloadSize { get; set; }

        const uint HEADERLEN = 2 * sizeof(uint) + 0; // size of PROCEEDING DATA (after this dword)                               
        //[TSOVoltronDBWrapperField] public uint RASHeader => 0x5F534152;        
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint RASLength2 => DecompressedSize + HEADERLEN;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint StreamSize2 => HB_PayloadSize - 0x4;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint StreamSize3 => HB_PayloadSize - 0x4;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint StreamSize4 => HB_PayloadSize - 0x4;
        //[TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public byte Filler2 => 0x01;

        const uint TSOSERIALIZABLESTREAM_HEAD_LEN = sizeof(uint) * 3 + 1;
        [TSOVoltronDBWrapperField] public byte CompressionMode { get; set; } = 0x01;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint DecompressedSize { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint CompressedSize { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint StreamBytesSize { get; set; }       

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDResponseTEST(uint houseID, TSODBHouseBlob BlobData) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            this.HouseID = houseID;
            HB_PayloadSize = BlobData.Length + HEADERLEN + TSOSERIALIZABLESTREAM_HEAD_LEN;
            CompressionMode = 0x01;
            DecompressedSize = BlobData.DecompressedSize;
            CompressedSize = StreamBytesSize = BlobData.Length;
            //DecompressedSize = CompressedSize; // patch!!!!
            StreamBytes = BlobData.BlobData;

            MakeBodyFromProperties();
        }
#else
        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDResponseTEST(uint houseID, TSODBHouseBlob BlobData) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            StreamBytes = BlobData.BlobData;

            MakeBodyFromProperties();
        }
#endif      
        [TSOVoltronDBWrapperField] public byte[] StreamBytes { get; set; }
    }
}

#define ATTEMPT_4

using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.DB;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    internal class TSOGetHouseBlobByIDResponseDEBUG : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {        
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

#if ATTEMPT_1        
        [TSOVoltronDBWrapperField] public uint Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint HB_PayloadSize { get; set; }

        const uint HEADERLEN = 2 * sizeof(uint) + 0; // size of PROCEEDING DATA (after this dword)                       
        //[TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint Filler2 => 0x02;      
        [TSOVoltronDBWrapperField] public uint RASHeader => 0x00;//0x5F534152;
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint RASLength2 => DecompressedSize + HEADERLEN;

#elif ATTEMPT_2
        const uint HEADERLEN = 0x0;
        [TSOVoltronDBWrapperField] public uint RASHeader => 0x5F534152;
        [TSOVoltronDBWrapperField] public byte Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint HB_PayloadSize { get; set; }
        
#elif ATTEMPT_3
        const uint HEADERLEN = 0x13;
        [TSOVoltronDBWrapperField] public byte Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint HB_PayloadSize { get; set; }
        [TSOVoltronDBWrapperField] public uint RASHeader => 0x5F534152;
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint RASLength => DecompressedSize + 0x20;
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint Filler2 => 0x02;
#else
        const uint HEADERLEN = 0x0;
#endif
        const uint TSOSERIALIZABLESTREAM_HEAD_LEN = sizeof(uint) * 3 + 1;
        [TSOVoltronDBWrapperField] public byte CompressionMode { get; set; } = 0x01;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint DecompressedSize { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint CompressedSize { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint StreamBytesSize { get; set; }
        [TSOVoltronDBWrapperField] public byte[] StreamBytes { get; set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDResponseDEBUG(uint houseID, TSODBHouseBlob BlobData) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            this.HouseID = houseID;
            //HB_PayloadSize = BlobData.Length + HEADERLEN + TSOSERIALIZABLESTREAM_HEAD_LEN;
            CompressionMode = 0x01;
            DecompressedSize = BlobData.DecompressedSize;
            CompressedSize = StreamBytesSize = BlobData.Length;
            DecompressedSize = CompressedSize; // patch!!!!
            StreamBytes = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }
}

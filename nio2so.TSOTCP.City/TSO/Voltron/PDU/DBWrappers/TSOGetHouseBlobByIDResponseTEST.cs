#define ATTEMPT_2

using nio2so.Formats.DB;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    internal class TSOGetHouseBlobByIDResponseTEST : TSODBRequestWrapper, ITSOSerializableStreamPDU
    { 
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public byte Filler1 { get; set; } = 0x01;

        //**

        [TSOVoltronDBWrapperField] public uint Filler4 => (uint)(0x1D + (StreamBytes?.Length ?? 0) + FOOTERLEN);
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.BigEndian)] public uint Filler5 => 0x5F534152;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint Filler6 => DecompressedSize + 0x11;
        [TSOVoltronDBWrapperField] public uint HB_Payload_Size { get; set; }

        //**TSOSERIALIZABLE

        const uint TSOSERIALIZABLESTREAM_HEAD_LEN = sizeof(uint) * 3 + 1;
        [TSOVoltronDBWrapperField] public byte CompressionMode { get; set; } = 0x01;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint DecompressedSize { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint CompressedSize { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint StreamBytesSize { get; set; }

        [TSOVoltronDBWrapperField] public byte[] StreamBytes { get; set; }
        //**FOOTER

        const uint FOOTERLEN = sizeof(uint) * 3;

        [TSOVoltronDBWrapperField] public uint Footer1 { get; set; } = 0x01;
        [TSOVoltronDBWrapperField] public uint Footer2 { get; set; } = 0xDADDE510;
        [TSOVoltronDBWrapperField] public uint Footer3 { get; set; } = 0xDADDE511;

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
            HB_Payload_Size = BlobData.Length + TSOSERIALIZABLESTREAM_HEAD_LEN;
            CompressionMode = 0x01;
            DecompressedSize = BlobData.DecompressedSize;
            CompressedSize = StreamBytesSize = BlobData.Length;
            StreamBytes = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }    
}

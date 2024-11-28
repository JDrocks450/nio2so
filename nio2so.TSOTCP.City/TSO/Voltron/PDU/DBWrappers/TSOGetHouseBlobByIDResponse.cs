#define ATTEMPT_1

using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    internal class TSOGetHouseBlobByIDResponse : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {        
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

#if ATTEMPT_1        
        [TSOVoltronDBWrapperField] public byte Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint PayloadSize { get; set; }

        const uint HEADERLEN = 3 * sizeof(uint); // size of PROCEEDING DATA (after this dword)        
        [TSOVoltronDBWrapperField] public uint RASHeader => 0x5F534152; 
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint Filler2 => 0x02;       
        [TSOVoltronDBWrapperField] [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint RASLength => DecompressedSize + HEADERLEN;        
#elif ATTEMPT_2
        const uint HEADERLEN = 0xD;
        [TSOVoltronDBWrapperField] public byte Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint PayloadSize { get; set; }
#elif ATTEMPT_3
        const uint HEADERLEN = 0x13;
        [TSOVoltronDBWrapperField] public byte Filler => 0x01;
        [TSOVoltronDBWrapperField] public uint PayloadSize { get; set; }
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
        public TSOGetHouseBlobByIDResponse(uint houseID, TSODBHouseBlob BlobData) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            this.HouseID = houseID;
            PayloadSize = BlobData.Length + HEADERLEN + TSOSERIALIZABLESTREAM_HEAD_LEN;
            CompressionMode = 0x01;
            DecompressedSize = BlobData.DecompressedSize;
            CompressedSize = StreamBytesSize = BlobData.Length + sizeof(uint);
            StreamBytes = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }
}

using MiscUtil.Conversion;
using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    internal class TSOGetHouseBlobByIDResponse : TSODBRequestWrapper
    {        
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public byte CompressionType { get; set; } = 0x01;
        [TSOVoltronDBWrapperField] public uint DecompressedSize { get; set; }
        [TSOVoltronDBWrapperField] public uint CompressedSize { get; set; }
        [TSOVoltronDBWrapperField] public uint BlobSize { get; set; }
        [TSOVoltronDBWrapperField] public byte[] HouseBlobData { get; set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDResponse(uint houseID, TSODBHouseBlob Blob) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            HouseID = houseID;
            DecompressedSize = CompressedSize = BlobSize = (uint)Blob.BlobData.Length;
            HouseBlobData = Blob.BlobData;

            MakeBodyFromProperties();
        }
    }
}

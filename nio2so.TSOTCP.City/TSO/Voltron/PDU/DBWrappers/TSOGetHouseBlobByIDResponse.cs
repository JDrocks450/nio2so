using MiscUtil.Conversion;
using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    internal class TSOGetHouseBlobByIDResponse : TSODBRequestWrapper
    {        
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDResponse(string AriesID, string MasterID, uint houseID, TSODBHouseBlob Blob) :
            base(
                    AriesID,
                    MasterID,
                    0x00,
                    0x49D8, //TSODBWrapperMessageSize.AutoSize,
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    0x21,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response,
                    CombineArrays(new byte[]
                    {
                        0x00,0x00,0x05,0x3A, // <--- HOUSEID
                        0x01,
                        0x00,0x01,0x02,0x03, // <--- FROM NIOTSO ... SEEMS RANDOM
                        0x04,0x05,0x06,0x07,
                        0x08,0x09,0x0A,0x0B,
                    },
                        //Next is the RAS stream (uint Size, byte[] RAS)
                        Blob.BlobData // <-- Stream here
                    )
                )
        {
            HouseID = houseID;            

            MoveBufferPositionToDBMessageBody();
            EmplaceBody(HouseID); // <--- overwrite houseid here for now            
        }
    }
}

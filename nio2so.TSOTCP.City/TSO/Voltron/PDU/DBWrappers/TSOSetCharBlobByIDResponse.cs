using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sends a confirmation to the Client that the <c>TSODBCharBlob</c> was received successfully
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response)]
    internal class TSOSetCharBlobByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField]
        public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField]
        public uint BlobSize { get; set; } = 0x00;
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] BlobDataStream { get; set; } = new byte[0];

        public TSOSetCharBlobByIDResponse(uint AvatarID, TSODBCharBlob BlobData) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response
                )
        {
            this.AvatarID = AvatarID;
            BlobSize = BlobData.BlobSize;
            BlobDataStream = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }
}

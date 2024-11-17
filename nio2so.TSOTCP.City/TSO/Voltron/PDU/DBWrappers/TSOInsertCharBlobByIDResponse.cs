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
    internal class TSOInsertCharBlobByIDResponse : TSODBRequestWrapper
    {
        public TSOInsertCharBlobByIDResponse(string ariesID,
                                             string masterID,
                                             uint NewAvatarID) : 
            base(ariesID,
                 masterID,
                 0x00,
                 TSODBWrapperMessageSize.AutoSize,
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 0x21,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Response,
                 new byte[] { })
        {
            MoveBufferPositionToDBMessageBody();

            EmplaceBody(NewAvatarID);
            EmplaceBody(0x01);
            ReadAdditionalMetadata();
        }
    }
}

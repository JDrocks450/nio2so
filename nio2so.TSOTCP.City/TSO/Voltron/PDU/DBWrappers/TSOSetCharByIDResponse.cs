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
    internal class TSOSetCharByIDResponse : TSODBRequestWrapper
    {
        public TSOSetCharByIDResponse(string ariesID,
                                             string masterID,
                                             uint AvatarID) : 
            base(ariesID,
                 masterID,
                 0x00,
                 TSODBWrapperMessageSize.AutoSize,
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 0x21,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Response,
                 new byte[] { })
        {
            MoveBufferPositionToDBMessageBody();

            EmplaceBody(AvatarID);
            EmplaceBody(0x01);
            ReadAdditionalMetadata();
        }
    }
}

using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sends a confirmation to the Client that the <c>TSODBCharBlob</c> was received successfully
    /// </summary>
    internal class TSOInsertCharBlobByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField]
        public uint NewAvatarID { get; set; }
        [TSOVoltronDBWrapperField]
        public uint StatusCode { get; set; } = 0x01;

        public TSOInsertCharBlobByIDResponse(uint NewAvatarID) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Response
                )
        {
            this.NewAvatarID = NewAvatarID;
            MakeBodyFromProperties();
        }
    }
}

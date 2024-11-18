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
        public TSOSetCharByIDResponse(uint AvatarID) : 
                                             
            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Response
                )
        {     
            this.AvatarID = AvatarID;       
            MakeBodyFromProperties();            
        }

        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0x01;
    }
}

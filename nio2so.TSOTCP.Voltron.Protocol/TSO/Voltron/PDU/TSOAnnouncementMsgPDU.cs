using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Structure matches what the game expects, but it will not show a message box on successful parse
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.ANNOUNCEMENT_MSG_PDU)]
    public class TSOAnnouncementMsgPDU : TSOVoltronPacket
    {
        public TSOAnnouncementMsgPDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOAnnouncementMsgPDU(TSOPlayerInfoStruct sender, string messageText)
        {
            Sender = sender;
            MessageText = messageText;
            //Message2 = message2;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// Sender info
        /// </summary>
        public TSOPlayerInfoStruct Sender { get; set; }
        public string MessageText { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.ANNOUNCEMENT_MSG_PDU;
    }
}

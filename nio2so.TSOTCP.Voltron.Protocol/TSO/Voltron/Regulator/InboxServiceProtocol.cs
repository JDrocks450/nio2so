using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles all requests pertaining to the Message Inbox in The Sims Online: Pre-Alpha
    /// </summary>
    [TSORegulator(nameof(InboxServiceProtocol))]
    internal class InboxServiceProtocol : TSOProtocol
    {
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_PDU)]
        public void GET_MPS_MESSAGES_PDU(TSOVoltronPacket PDU)
        { // get all of my messages
            return;
            RespondWith(new TSOGetMPSMessagesPDUResponse());
        }
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU)]
        public void FLASH_MESSAGE_PDU(TSOVoltronPacket PDU)
        {
            TSOFlashMessagePDU messagePDU = (TSOFlashMessagePDU)PDU;
            string[] strings = messagePDU.GetContentStrings();

            RespondWith(new TSOFlashMessageResponsePDU(messagePDU.RecipientID, messagePDU.PlayerInfo, messagePDU.PackedContent));
            RespondWith(
                new TSOFlashMessagePDU(
                    new TSOPlayerInfoStruct(new TSOAriesIDStruct(TestingConstraints.MyFriendAvatarID, TestingConstraints.MyFriendAvatarName)),
                    messagePDU.PlayerInfo.PlayerID, "please stop contacting me."
                ));
            return;
            
            //DELETE MPS MESSAGE?
        }
    }
}

using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.Services;
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

            uint recipientID = ((ITSONumeralStringStruct)messagePDU.RecipientID).NumericID ?? 0;
            if (recipientID == 0) return; // todo: handle error with failed pdu

            nio2soVoltronDataServiceClient dataService = GetService<nio2soVoltronDataServiceClient>();
            string recipientName = dataService.GetAvatarNameByAvatarID(recipientID).Result;

            //try to locate and send the SMS message to the client
            if (TrySendTo(messagePDU.RecipientID, new TSOFlashMessagePDU(messagePDU.PlayerInfo, new(recipientID, recipientName), messagePDU.PackedContent, messagePDU.MessageType)))
            {
                RespondWith(new TSOFlashMessageResponsePDU(messagePDU.RecipientID, messagePDU.PlayerInfo, messagePDU.PackedContent));
                return;
            }
            //handle client no longer connected to Voltron below with FlashMsgFailed response pdu.
        }
    }
}

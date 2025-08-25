using nio2so.DataService.Common.Types;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    /// <summary>
    /// Handles all requests pertaining to the Message Inbox in The Sims Online: Pre-Alpha
    /// </summary>
    [TSORegulator(nameof(InboxServiceProtocol))]
    internal class InboxServiceProtocol : TSOProtocol
    {
        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_PDU)]
        public void GET_MPS_MESSAGES_PDU(TSOVoltronPacket PDU)
        { // return an empty status message response 
            //try to identify the client first to determine who we are getting messages for
            if (!GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID) || VoltronID == default)
            {
                RespondWith(new TSOGetMPSMessagesResponsePDU(TSOStatusReasonStruct.Offline));
                return;
            }
            //try to download their messages from the data service
            if (!TryDataServiceQuery(x => x.GetInboxMessages(VoltronID.AvatarID), out IEnumerable<Letter>? Messages, out string ReasonText) || Messages == default)
            {
                RespondWith(new TSOGetMPSMessagesResponsePDU(new TSOStatusReasonStruct(1, ReasonText)));
                return;
            }
            //respond that the messages are ready
            RespondWith(new TSOGetMPSMessagesResponsePDU(TSOStatusReasonStruct.Default));
            //send each message as a flash message pdu
            foreach (var message in Messages)
                RespondWith(FromLetter(message));
            //clear their inbox after sending messages
            GetDataService().ClearInboxMessages(VoltronID.AvatarID);
        }

        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU)]
        public void FLASH_MESSAGE_PDU(TSOVoltronPacket PDU)
        {
            TSOFlashMessagePDU messagePDU = (TSOFlashMessagePDU)PDU;
            string[] strings = messagePDU.GetContentStrings();

            uint recipientID = ((ITSONumeralStringStruct)messagePDU.RecipientID).NumericID ?? 0;
            if (recipientID == 0) return; // todo: handle error with failed pdu
            uint senderID = messagePDU.PlayerInfo.PlayerID.AvatarID;
            if (senderID == 0) return; // todo: handle error with failed pdu

            nio2soVoltronDataServiceClient dataService = GetService<nio2soVoltronDataServiceClient>();
            string recipientName = dataService.GetAvatarNameByAvatarID(recipientID).Result;

            void logError()
            {
                LogConsole($"Failed to deliver {messagePDU.MessageType} flash message from {messagePDU.PlayerInfo.PlayerID} to {messagePDU.RecipientID}.");
            }

            switch (messagePDU.MessageType)
            {
                case TSOFlashMessagePDU.FlashTypes.SMS:
                    //try to locate and send the SMS message to the client
                    if (TrySendTo(messagePDU.RecipientID, new TSOFlashMessagePDU(messagePDU.PlayerInfo, new(recipientID, recipientName), messagePDU.PackedContent, messagePDU.MessageType)))
                    {
                        RespondWith(new TSOFlashMessageResponsePDU(messagePDU.RecipientID, messagePDU.PlayerInfo, messagePDU.PackedContent));
                        break;
                    }
                    logError();
                    //handle client no longer connected to Voltron below with FlashMsgFailed response pdu.
                    break;
                case TSOFlashMessagePDU.FlashTypes.Letter:
                    //try to deliver this message to this person's inbox
                    if (GetDataService().SendInboxMessageToAvatar(senderID, recipientID, ToLetter(messagePDU)).Result.IsSuccessStatusCode)
                    {
                        RespondWith(new TSOFlashMessageResponsePDU(messagePDU.RecipientID, messagePDU.PlayerInfo, messagePDU.PackedContent));
                        break;
                    }
                    logError();
                    //handle client no longer connected to Voltron below with FlashMsgFailed response pdu.                    
                    break;
            }
        }

        Letter ToLetter(in TSOFlashMessagePDU LetterPDU)
        {
            if (LetterPDU.MessageType != TSOFlashMessagePDU.FlashTypes.Letter)
                throw new InvalidOperationException("Cannot convert non-letter Flash Message PDU to Letter type.");
            return new Letter(            
                LetterPDU.PlayerInfo.PlayerID.AvatarID,
                LetterPDU.RecipientID.AvatarID,  
                LetterPDU.LetterSenderName,
                LetterPDU.LetterTitle,
                LetterPDU.MessageBody,
                LetterPDU.SentTime
            );
        }
        TSOFlashMessagePDU FromLetter(in Letter DataServiceLetter)
        {
            AvatarProtocol avatarProtocol = GetRegulator<AvatarProtocol>();
            return TSOFlashMessagePDU.MakeLetter(
                avatarProtocol.GetPlayerInfoStruct(DataServiceLetter.SenderID),
                avatarProtocol.GetVoltronIDStruct(DataServiceLetter.ReceiverID),                
                DataServiceLetter.Title,
                DataServiceLetter.Body,
                DataServiceLetter.SentTime,
                DataServiceLetter.SenderDisplayName
            );
        }
    }
}

using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU)]
    public class TSOFlashMessagePDU : TSOVoltronPacket
    {
        public const int EXPECTED_STRINGS = 5;        

        /// <summary>
        /// Types of flash messages
        /// </summary>
        public enum FlashTypes : ushort
        {
            /// <summary>
            /// A non-persistent message
            /// </summary>
            SMS,
            /// <summary>
            /// A persistent message
            /// </summary>
            Letter
        }
        /// <summary>
        /// The type of message received. See: <see cref="FlashTypes"/>
        /// </summary>
        public FlashTypes MessageType { get; set; } = FlashTypes.SMS;
        public string ReasonText { get; set; } = "";
        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the sender of the message
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; } = new();

        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the intended recipient of the message
        /// </summary>
        public TSOAriesIDStruct RecipientID { get; set; } = TSOAriesIDStruct.Default;
        /// <summary>
        /// Packed <see cref="IsLetter"/>, <see cref="SentTime"/>, <see cref="LetterSenderName"/>, <see cref="LetterTitle"/>, and <see cref="MessageBody"/>
        /// </summary>

        [TSOVoltronString]
        public string? PackedContent { get; set; } = "";

        /// <summary>
        /// Creates a new <see cref="TSOFlashMessagePDU"/> with <see langword="default"/> values
        /// </summary>
        public TSOFlashMessagePDU() : base()
        {
            MakeBodyFromProperties();
        }

        /// <summary>
        /// Creates a new <see cref="TSOFlashMessagePDU"/> with selected values
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <param name="recipientID"></param>
        /// <param name="MessageContent"></param>
        /// <param name="MessageType"></param>
        public TSOFlashMessagePDU(TSOPlayerInfoStruct playerInfo, TSOAriesIDStruct recipientID, string MessageContent, FlashTypes MessageType = FlashTypes.SMS) : this()
        {
            PlayerInfo = playerInfo;
            RecipientID = recipientID;
            PackedContent = MessageContent;
            MakeBodyFromProperties();
        }

        //**AUTO POPULATER

        /// <summary>
        /// 0 = SMS 1 = Letter
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public bool IsLetter => GetContentStrings()[0] != "0";
        /// <summary>
        /// The time this message was sent -- populated in SMS and Letter messages
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string SentTime => GetContentStrings()[1];
        /// <summary>
        /// Letter sender name -- only available in letters
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string LetterSenderName => GetContentStrings()[2];
        /// <summary>
        /// Letter title -- only available in letters
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string LetterTitle => GetContentStrings()[3];
        /// <summary>
        /// Message text, true for both SMS and Letter messages
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string MessageBody => GetContentStrings()[4];

        /// <summary>
        /// Breaks down <see cref="PackedContent"/> into <see cref="EXPECTED_STRINGS"/> strings which can be accessed using properties
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public string[] GetContentStrings()
        {
            var strings = PackedContent.Split((char)0x01);
            if (strings.Length != EXPECTED_STRINGS) 
                throw new InvalidDataException($"Corrupted message content? Expected {EXPECTED_STRINGS} string(s) - got {strings.Length}");
            return strings;
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU;
    }
}

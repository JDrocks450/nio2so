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

        public enum FlashTypes : ushort
        {
            SMS,
            Letter
        }
        /// <summary>
        /// The type of message received. See: <see cref="FlashTypes"/>
        /// </summary>
        public FlashTypes MessageType { get; set; }
        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the sender of the message
        /// </summary>
        public TSOAriesIDStruct SenderID { get; set; }
        /// <summary>
        /// The masterID of the sender
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// Appears to be 0x41
        /// </summary>
        public byte Byte1 { get; set; }
        /// <summary>
        /// Appears to be 0x01
        /// </summary>
        public byte Byte2 { get; set; } = 0x1;
        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the intended recipient of the message
        /// </summary>
        public TSOAriesIDStruct RecipientID { get; set; }
        /// <summary>
        /// Packed <see cref="IsLetter"/>, <see cref="SentTime"/>, <see cref="LetterSenderName"/>, <see cref="LetterTitle"/>, and <see cref="MessageBody"/>
        /// </summary>

        [TSOVoltronString]
        public string PackedContent { get; set; }

        //**AUTO POPULATER

        /// <summary>
        /// 0 = SMS 1 = Letter
        /// </summary>
        [IgnoreDataMember]
        public bool IsLetter => GetContentStrings()[0] != "0";
        /// <summary>
        /// The time this message was sent -- populated in SMS and Letter messages
        /// </summary>
        [IgnoreDataMember]
        public string SentTime => GetContentStrings()[1];
        /// <summary>
        /// Letter sender name -- only available in letters
        /// </summary>
        [IgnoreDataMember]
        public string LetterSenderName => GetContentStrings()[2];
        /// <summary>
        /// Letter title -- only available in letters
        /// </summary>
        [IgnoreDataMember]
        public string LetterTitle => GetContentStrings()[3];
        /// <summary>
        /// Message text, true for both SMS and Letter messages
        /// </summary>
        [IgnoreDataMember]
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

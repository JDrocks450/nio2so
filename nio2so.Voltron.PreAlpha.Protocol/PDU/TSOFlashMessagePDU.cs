using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU)]
    public class TSOFlashMessagePDU : TSOVoltronPacket
    {
        public const char SEPARATOR = '\u0001';
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
            Letter = 256
        }
        /// <summary>
        /// The type of message received. See: <see cref="FlashTypes"/>
        /// </summary>
        public FlashTypes MessageType { get; set; } = FlashTypes.SMS;
        public string ReasonText { get; set; } = "";

        /// <summary>
        /// The <see cref="TSOPlayerInfoStruct"/> of the sender of the message
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; } = new();

        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the intended recipient of the message
        /// </summary>
        public TSOAriesIDStruct RecipientID { get; set; } = TSOAriesIDStruct.Default;
        /// <summary>
        /// Packed <see cref="IsLetter"/>, <see cref="SentTime"/>, <see cref="LetterSenderName"/>, <see cref="LetterTitle"/>, and <see cref="MessageBody"/>
        /// <para/>Note: This format is sometimes changed to have different amounts of packed strings, which makes this harder to formulate.
        /// <para/>Joining a lot without a host on it will result in a <see cref="TSOFlashMessagePDU"/> being sent with only one string.
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
        /// Creates a new <see cref="TSOFlashMessagePDU"/> with selected values. <para/>
        /// <see cref="MakeLetter(TSOPlayerInfoStruct, TSOAriesIDStruct, string, string, DateTime)"/>
        /// </summary>
        /// <param name="playerInfo"> The sender of this message </param>
        /// <param name="recipientID">The intended recipient of this message</param>
        /// <param name="PackedContent">The content of the message. See: <see cref="PackedContent"/><para/><inheritdoc cref="PackedContent"/></param>
        /// <param name="MessageType">The type of FlashMsg we're sending</param>
        public TSOFlashMessagePDU(TSOPlayerInfoStruct playerInfo, TSOAriesIDStruct recipientID, string PackedContent, FlashTypes MessageType = FlashTypes.SMS) : this()
        {
            PlayerInfo = playerInfo;
            RecipientID = recipientID;
            this.PackedContent = PackedContent;

            MakeBodyFromProperties();
        }
        /// <summary>
        /// Creates a new <see cref="TSOFlashMessagePDU"/> formatted as a letter.
        /// </summary>
        /// <param name="Sender">The sender of this letter</param>
        /// <param name="Recipient">The receiver of this letter</param>
        /// <param name="Title">The title of this letter</param>
        /// <param name="Body">The content of the body of this letter</param>
        /// <param name="SentTime">The time the letter was sent</param>
        /// <returns></returns>
        public static TSOFlashMessagePDU MakeLetter(TSOPlayerInfoStruct Sender, TSOAriesIDStruct Recipient, string Title, string Body, DateTime SentTime, string? DisplayName) => 
            new TSOFlashMessagePDU(Sender, Recipient, $"1{SEPARATOR}" +
                $"{encodeDate(SentTime)}{SEPARATOR}" +
                $"{DisplayName ?? Sender.PlayerID.MasterID.Replace(((ITSONumeralStringStruct)Sender.PlayerID).FormatSpecifier, "")}{SEPARATOR}" +
                $"{Title}{SEPARATOR}{Body}", FlashTypes.Letter);

        //**AUTO POPULATER

        /// <summary>
        /// "0" = SMS, "1" = Letter
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public bool IsLetter => GetContentStrings()[0] != "0";
        /// <summary>
        /// The time this message was sent -- populated in SMS and Letter messages
        /// <code>Format : YYYY:MM:DD:HH:MM:SS</code>
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public DateTime SentTime => decodeDate(GetContentStrings().ElementAtOrDefault(1));
        /// <summary>
        /// <code>Format : YYYY:MM:DD:HH:MM:SS</code>
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        static string encodeDate(DateTime dateTime) => $"{dateTime.Year}:{dateTime.Month}:{dateTime.Day}:{dateTime.Hour}:{dateTime.Minute}:{dateTime.Second}";
        /// <summary>
        /// <code>Format : YYYY:MM:DD:HH:MM:SS</code>
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        static DateTime decodeDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return DateTime.MinValue;
            var parts = dateString.Split(':');
            if (parts.Length != 6) return DateTime.MinValue;
            return new DateTime(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[3]),
                int.Parse(parts[4]),
                int.Parse(parts[5])
            );
        }
        /// <summary>
        /// Letter sender name -- only available in letters
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string? LetterSenderName => GetContentStrings().ElementAtOrDefault(2);
        /// <summary>
        /// Letter title -- only available in letters
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string? LetterTitle => GetContentStrings().ElementAtOrDefault(3);
        /// <summary>
        /// Message text, true for both SMS and Letter messages
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public string MessageBody => GetContentStrings().Last();

        /// <summary>
        /// Breaks down <see cref="PackedContent"/> into <see cref="EXPECTED_STRINGS"/> strings which can be accessed using properties
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public string[] GetContentStrings()
        {
            var strings = PackedContent?.Split(SEPARATOR) ?? [];        
            return strings;
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU;
    }
}

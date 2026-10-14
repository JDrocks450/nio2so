using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.DataService
{
    public class TSOServiceMessage
    {
        const int WIDTH = sizeof(TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs) + (2*sizeof(ushort));

        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>       
        [TSOVoltronArrayLength(nameof(MessageContentBytes),-WIDTH)]
        public uint ContentLength { get; set; }
        /// <summary>
        /// The format of this <see cref="MessageContentBytes"/>
        /// </summary>
        public TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs MessageFormat { get; set; }

        public ushort Unknown_Flags { get; set; } = 0x2F4C;
        /// <summary>
        /// The size, in bytes, of this <see cref="TSOServiceMessage"/> object
        /// <para/>The byte preceeding the pascal string's length is not counted
        /// </summary>
        public ushort ParameterLength { get; set; } = 0;
        
        [TSOVoltronBodyArray]
        public byte[] MessageContentBytes { get; set; } = [];

        public TSOServiceMessage()
        {

        }

        public TSOServiceMessage(ITSOMessage MessageContent, TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs? MessageFormat = default) : this()
        {
            SetMessageAs(MessageContent);
            this.MessageFormat = MessageFormat ?? (MessageContent is TSOTopicUpdateMessage ? TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs.cTSOTopicUpdateMessage : TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs.cTSONetMessageStandard);
            if (MessageFormat == TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs.cTSONetMessageStandard && MessageContent is not TSONetMessageStandard)
                throw new InvalidCastException("The MessageContent type could not be verified.");
        }

        /// <summary>
        /// Attempts to read the message enclosed in this <see cref="TSOServiceMessage"/> as the provided <see cref="ITSOMessage"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public T GetMessageAs<T>() where T : class, ITSOMessage, new()
        {
            return TSOVoltronSerializer.Deserialize<T>(MessageContentBytes) as T ??
                throw new NullReferenceException($"Deserialized {typeof(T).Name} failed!");
            throw new NotImplementedException(typeof(T).Name);
        }

        public void SetMessageAs<T>(T Message) where T : ITSOMessage => 
            MessageContentBytes = TSOVoltronSerializer.Serialize<T>(Message);

        public override string ToString()
        {
            return $"{GetType().Name} {{ {nameof(ContentLength)}: {ContentLength}, {nameof(MessageContentBytes)}.Length: {MessageContentBytes.Length} (reported: {ContentLength-WIDTH}) " +
                $"{nameof(MessageFormat)}: {MessageFormat}";
        }
    }
}

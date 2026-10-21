using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.DataService
{
    public class TSOServiceMessage
    {
        const int WIDTH = sizeof(TSO_NewImproved_TSOMessageType_CLSIDs) + (2*sizeof(ushort));
        
        /// <summary>
        /// The format of this <see cref="MessageContentBytes"/>
        /// </summary>
        public TSO_NewImproved_TSOMessageType_CLSIDs MessageFormat { get; set; }

        public uint SequenceID { get; set; } = 0x2EEC9157;

        [TSOVoltronBodyArray]
        public byte[] MessageContentBytes { get; set; } = [];

        public TSOServiceMessage()
        {

        }

        public TSOServiceMessage(ITSOMessage MessageContent, uint SequenceID) : this()
        {
            SetMessageAs(MessageContent);
            this.SequenceID = SequenceID;
        }

        /// <summary>
        /// <inheritdoc cref="TSOMessageFormatBase.GetMessageAs{T}(byte[])"/>
        /// <para/> See: <see cref="TSOMessageFormatBase.GetMessageAs{T}(byte[])"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetMessageAs<T>() where T : TSOMessageFormatBase, new() => TSOMessageFormatBase.GetMessageAs<T>(MessageContentBytes);
        /// <summary>
        /// <inheritdoc cref="TSOMessageFormatBase.GetMessageAs(TSO_NewImproved_TSOMessageType_CLSIDs, byte[])"/>
        /// <para/> See: <see cref="TSOMessageFormatBase.GetMessageAs(TSO_NewImproved_TSOMessageType_CLSIDs, byte[])"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ITSOMessage GetMessageAs() => TSOMessageFormatBase.GetMessageAs(MessageFormat, MessageContentBytes);

        public void SetMessageAs<T>(T Message) where T : ITSOMessage
        {
            MessageContentBytes = TSOVoltronSerializer.Serialize<T>(Message);
            MessageFormat = Message.GetMessageType();
        }

        public override string ToString()
        {
            return $"{GetType().Name} {{ {nameof(SequenceID)}: {SequenceID} {nameof(MessageContentBytes)}.Length: {MessageContentBytes.Length} " +
                $"{nameof(MessageFormat)}: {MessageFormat}";
        }
    }
}

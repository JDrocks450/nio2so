using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{
    public interface ITSOMessage
    {
        TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType();
        /// <summary>
        /// The number assigned to this Request/Response
        /// <para/>See: <see cref="ITSOMessage.GetResponseID"/>
        /// </summary>
        uint GetResponseID();
        /// <summary>
        /// Sets the number assigned to this Request/Response
        /// <para/>See: <see cref="ITSOMessage.GetResponseID"/>
        /// </summary>
        void SetResponseID(uint NewResponseID);
    }
    public abstract class TSOMessageFormatBase : ITSOMessage
    {
        public abstract TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType();

        /// <summary>
        /// Attempts to read the message enclosed in this <see cref="TSOServiceMessage"/> as the provided <see cref="ITSOMessage"/> <paramref name="MessageType"/>
        /// </summary>
        /// <param name="MessageType"></param>
        /// <param name="MessageContentBytes"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ITSOMessage GetMessageAs(TSO_NewImproved_TSOMessageType_CLSIDs MessageType, byte[] MessageContentBytes)
        {
            
            switch (MessageType)
            {
                case TSO_NewImproved_TSOMessageType_CLSIDs.cTSONetMessageStandard:
                    return GetMessageAs<TSONetMessageStandard>(MessageContentBytes);
                    
                case TSO_NewImproved_TSOMessageType_CLSIDs.cTSOTopicUpdateMessage:
                    return GetMessageAs<TSOTopicUpdateMessage>(MessageContentBytes);

                case TSO_NewImproved_TSOMessageType_CLSIDs.cTSOAvatarCreationRequest:
                    return GetMessageAs<TSOAvatarCreationRequest>(MessageContentBytes);

                default:
                    throw new NotImplementedException($"{MessageType} isn't implemented yet. Better get on that now.");
            }
        }
        /// <summary>
        /// Attempts to read the message enclosed in this <see cref="TSOServiceMessage"/> as the provided <see cref="ITSOMessage"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static T GetMessageAs<T>(byte[] MessageContentBytes) where T : TSOMessageFormatBase, new()
        {
            return TSOVoltronSerializer.Deserialize<T>(MessageContentBytes) as T ??
                throw new NullReferenceException($"Deserialized {typeof(T).Name} failed!");
            throw new NotImplementedException(typeof(T).Name);
        }

        public abstract uint GetResponseID();
        public abstract void SetResponseID(uint NewResponseID);
    }
}

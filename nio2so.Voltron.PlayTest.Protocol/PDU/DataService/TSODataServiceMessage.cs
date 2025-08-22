using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.PlayTest.Protocol.PDU.MessageFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PlayTest.Protocol.PDU.DataService
{
    public class TSODataServiceMessage
    {
        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>       
        [TSOVoltronArrayLength(nameof(MessageContentBytes),sizeof(TSO_PlayTest_MsgCLSIDs))]
        public uint ContentLength { get; set; }
        /// <summary>
        /// The format of this <see cref="TSODataServiceMessage.MessageContentBytes"/>
        /// </summary>
        public TSO_PlayTest_MsgCLSIDs MessageFormat { get; set; }
        /// <summary>
        /// Unknown what this stands for
        /// </summary>
        public uint Unknown { get; set; } = 0x953C001D;
        [TSOVoltronBodyArray]
        public byte[] MessageContentBytes { get; set; } = [];
        /// <summary>
        /// Attempts to read the message enclosed in this <see cref="TSODataServiceMessage"/> as the provided <see cref="ITSOMessage"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public T GetMessageAs<T>() where T : class, ITSOMessage
        {
            if (typeof(T) == typeof(TSONetMessageStandard))            
                return TSOVoltronSerializer.Deserialize<TSONetMessageStandard>(MessageContentBytes) as T ?? 
                    throw new NullReferenceException($"Deserialized {typeof(T).Name} failed!");            
            throw new NotImplementedException(typeof(T).Name);
        }

        public override string ToString()
        {
            return $"{GetType().Name} {{ {nameof(ContentLength)}: {ContentLength}, " +
                $"{nameof(MessageFormat)}: {MessageFormat}";
        }
    }
}

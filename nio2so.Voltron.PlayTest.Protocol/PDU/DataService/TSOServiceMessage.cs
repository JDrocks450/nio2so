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
    public class TSOServiceMessage
    {
        const int WIDTH = sizeof(TSO_PlayTest_MsgCLSIDs) + sizeof(uint);


        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>       
        [TSOVoltronArrayLength(nameof(MessageContentBytes),-WIDTH)]
        public uint ContentLength { get; set; }
        /// <summary>
        /// The format of this <see cref="TSOServiceMessage.MessageContentBytes"/>
        /// </summary>
        public TSO_PlayTest_MsgCLSIDs MessageFormat { get; set; }        
        public ushort Unknown_Flags { get; set; } = 0x2F4C;
        /// <summary>
        /// The size, in bytes, of this <see cref="TSOServiceMessage"/> object
        /// <para/>The byte preceeding the pascal string's length is not counted
        /// </summary>
        public ushort ParameterLength { get; set; } = 0;
        public byte[] MessageContentBytes { get; set; } = [];
        /// <summary>
        /// Attempts to read the message enclosed in this <see cref="TSOServiceMessage"/> as the provided <see cref="ITSOMessage"/> <typeparamref name="T"/>
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
            return $"{GetType().Name} {{ {nameof(ContentLength)}: {ContentLength}, {nameof(MessageContentBytes)}.Length: {MessageContentBytes.Length} (reported: {ContentLength-WIDTH}" +
                $"{nameof(MessageFormat)}: {MessageFormat}";
        }
    }
}

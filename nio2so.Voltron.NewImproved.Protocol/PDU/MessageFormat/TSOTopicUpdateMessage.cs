using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{
    internal partial class TSOTopicUpdateMessage : ITSOMessage
    {
        public record UInt32Wrapper(uint Value);

        public TSOTopicUpdateMessage() {
        
        }

        public TSOTopicUpdateMessage(TSOValueBase value, params uint[] VectorData)
        {
            //UpdateCounter = updateCounter;
            this.Vector = VectorData.Select(x => new UInt32Wrapper(x)).ToArray();
            Value = value;
        }
        public TSOTopicUpdateMessage(TSOValueBase value, uint TSODataDefinitionFieldID, params uint[] AdditionalVectorData) : this(value, [..AdditionalVectorData,TSODataDefinitionFieldID])
        {

        }

        //public uint UpdateCounter { get; set; }
        public TSO_NewImproved_GZMSGIDs MessageID { get; set; } = TSO_NewImproved_GZMSGIDs.GZMSGID_cITSOApprovedTopicUpdate;
        public uint StatusCode { get; set; } = 0;
        [TSOVoltronArrayLength(nameof(Vector))]
        public uint VectorCount { get; set; }
        public UInt32Wrapper[] Vector { get; set; } = Array.Empty<UInt32Wrapper>();

        public TSOValueBase Value { get; set; }

        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string StatusMessage { get; set; } = "";
    }
}

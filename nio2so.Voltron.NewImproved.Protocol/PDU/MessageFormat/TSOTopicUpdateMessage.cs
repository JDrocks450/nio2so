using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{
    internal partial class TSOTopicUpdateMessage : TSOMessageFormatBase
    {
        const TSO_NewImproved_TSOMessageType_CLSIDs MYID = TSO_NewImproved_TSOMessageType_CLSIDs.cTSOTopicUpdateMessage;

        public TSOTopicUpdateMessage() {
        
        }

        public TSOTopicUpdateMessage(ITSOValue value, params uint[] VectorData)
        {
            //UpdateCounter = updateCounter;
            this.Vector = VectorData;
            Value = value;
        }

        public TSOTopicUpdateMessage(ITSOValue value, uint TSODataDefinitionFieldID, uint TSODataDefinitionTypeID, uint ObjectID) : this(value, [TSODataDefinitionTypeID, ObjectID, TSODataDefinitionFieldID])
        {

        }

        //public uint UpdateCounter { get; set; } = 0x2EEC92A5;
        public TSO_NewImproved_GZMSGIDs MessageID { get; set; } = TSO_NewImproved_GZMSGIDs.GZMSGID_cITSOApprovedTopicUpdate;
        public uint StatusCode { get; set; } = 0;
        [TSOVoltronArrayLength(nameof(Vector))]
        public uint VectorCount { get; set; }
        public uint[] Vector { get; set; } = Array.Empty<uint>();
        [TSOVoltronCustomDeserializationBehavior(typeof(TSOValueBase<object>),nameof(TSOValueBase<object>.OnDeserialize))]
        public ITSOValue Value { get; set; }

        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string StatusMessage { get; set; } = "";

        public uint GetTSODataDefinitionDetail_TypeID() => Vector[0];
        public uint GetTSODataDefinitionDetail_ObjectID() => Vector[1];
        public uint GetTSODataDefinitionDetail_FieldID() => Vector[2];

        public override TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType() => MYID;
    }
}

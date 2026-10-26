using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{
    /// <summary>
    /// A <see cref="ITSOMessage"/> format that is a container for an <see cref="ITSOValue"/> type on a given type found in the TSO Data Definition
    /// <para/>You can use <see cref="TSO_NewImproved_TSOData_Constants"/> for a list of all constants, many of which are TSO Data Objects
    /// <para/>This will update the client with the value of a given Field (See: <see cref="TSOTopicUpdateMessage.GetTSODataDefinitionDetail_FieldID()"/>) on a TSO Data Object
    /// <para/>Please be aware: TSOClient can sometimes send a value that isn't a <see cref="TSOValue_TSODataStructProperty"/> to a type expected to be an Data Struct type.
    /// In these circumstances, it is understood we should utilize the value that is responded with to make a Struct from that(those) value(s)
    /// </summary>
    internal partial class TSOTopicUpdateMessage : TSOMessageFormatBase
    {
        const TSO_NewImproved_TSOMessageType_CLSIDs MYID = TSO_NewImproved_TSOMessageType_CLSIDs.cTSOTopicUpdateMessage;

        /// <summary>
        /// Parameterless constructure for Serialization .. do not use.
        /// </summary>
        public TSOTopicUpdateMessage() : base()
        {

        }
        /// <summary>
        /// Creates a new <see cref="TSOTopicUpdateMessage"/> with the given <paramref name="value"/> and manual Vector parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="VectorData"></param>
        public TSOTopicUpdateMessage(ITSOValue value, params uint[] VectorData) : this()
        {
            this.Vector = VectorData;
            Value = value;
        }
        /// <summary>
        /// Creates a new <see cref="TSOTopicUpdateMessage"/> with the given <paramref name="value"/> and automatically
        /// sets the appropriate <see cref="Vector"/> values for you.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="TSODataDefinitionFieldID"></param>
        /// <param name="TSODataDefinitionTypeID"></param>
        /// <param name="ObjectID"></param>
        public TSOTopicUpdateMessage(ITSOValue value,
                                     uint TSODataDefinitionFieldID,
                                     uint TSODataDefinitionTypeID,
                                     uint ObjectID) : this(value, [TSODataDefinitionTypeID, ObjectID, TSODataDefinitionFieldID])
        {

        }

        /// <summary>
        /// The type of <see cref="TSOTopicUpdateMessage"/> -- should be <see cref="TSO_NewImproved_GZMSGIDs.GZMSGID_cITSOApprovedTopicUpdate"/>
        /// </summary>
        public TSO_NewImproved_GZMSGIDs MessageID { get; set; } = TSO_NewImproved_GZMSGIDs.GZMSGID_cITSOApprovedTopicUpdate;
        /// <summary>
        /// Optional status code, never seen this used.
        /// </summary>
        public uint StatusCode { get; set; } = 0;
        /// <summary>
        /// The amount of items in the following <see cref="Vector"/> -- automatically populated by <see cref="TSOVoltronSerializer"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(Vector))]
        public uint VectorCount { get; set; }
        /// <summary>
        /// Fields on the <see cref="TSOTopicUpdateMessage"/> giving context around the ObjectID, FieldID, TypeID, etc.
        /// <para/>See: <see cref="GetTSODataDefinitionDetail_FieldID"/>
        /// </summary>
        public uint[] Vector { get; set; } = Array.Empty<uint>();
        /// <summary>
        /// The <see cref="ITSOValue"/> on this <see cref="TSOTopicUpdateMessage"/>
        /// </summary>
        [TSOVoltronCustomDeserializationBehavior(typeof(TSOValueBase<object>),nameof(TSOValueBase<object>.OnDeserialize))]
        public ITSOValue Value { get; set; }

        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string StatusMessage { get; set; } = "";

        public uint GetTSODataDefinitionDetail_TypeID() => Vector[0];
        public uint GetTSODataDefinitionDetail_ObjectID() => Vector[1];
        public uint GetTSODataDefinitionDetail_FieldID() => Vector[2];

        public override TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType() => MYID;

        public override string ToString()
        {
            return $"{nameof(TSOTopicUpdateMessage)} TSODataType:{(TSO_NewImproved_TSOData_Constants)GetTSODataDefinitionDetail_TypeID()} " +
                $"ObjectID: {GetTSODataDefinitionDetail_ObjectID()} " +
                $"FieldID: {(TSO_NewImproved_TSOData_Constants)GetTSODataDefinitionDetail_FieldID()} " +
                $"with: {Value}";
        }
    }
}

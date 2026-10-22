using nio2so.Formats.TSOData;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using System.Dynamic;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    /// <summary>
    /// After further research this should probably be moved into <see cref="TSOValueBase{T}"/>
    /// <para/> Encompasses the String ID of the field of the data type found in the TSODataDefinition, then a <see cref="ITSOValue"/> which is the value.
    /// </summary>
    [Serializable]
    internal class TSOValue_DataFieldValueWrapper
    {
        public TSOValue_DataFieldValueWrapper() { }

        public TSOValue_DataFieldValueWrapper(uint TSODataDefinitionFieldID, ITSOValue TSOValue)
        {
            this.TSODataDefinitionFieldID = TSODataDefinitionFieldID;
            this.TSOValue = TSOValue;
        }

        public uint TSODataDefinitionFieldID { get; set; }        
        [TSOVoltronCustomDeserializationBehavior(typeof(TSOValueBase<object>), nameof(TSOValueBase<object>.OnDeserialize))]
        public ITSOValue TSOValue { get; set; }
    }

    /// <summary>
    /// Encaptulates the individual fields on a data type found in the TSODataDefinition for use in <see cref="TSODataServiceWrapperPDU"/>
    /// </summary>
    [Serializable]
    internal class TSOValue_TSODataStructPropertyWrapper
    {
        public TSOValue_TSODataStructPropertyWrapper()
        {
        }

        /// <summary>
        /// Creates a new <see cref="TSOValue_TSODataStructPropertyWrapper"/> with the given TSODataDefinition String ID representing the structure, 
        /// and the given fields on that structure.
        /// </summary>
        /// <param name="TSODataDefinition_TypeStructID"></param>
        /// <param name="fields"></param>
        public TSOValue_TSODataStructPropertyWrapper(uint TSODataDefinition_TypeStructID, params TSOValue_DataFieldValueWrapper[] fields)
        {
            this.TSODataDefinition_TypeStructID = TSODataDefinition_TypeStructID;
            Fields = fields;
        }
        /// <summary>
        /// This should not be changed in code.
        /// </summary>
        public TSO_NewImproved_TSOValueBase_CLSIDs TSOPropertyCLSID { get; set; } = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOProperty;
        /// <summary>
        /// The StringID of the TSODataDefinition Property we're encaptulating
        /// <para/>This is a property of a data struct defined in the TSODataDefinition, the ID of the Struct is captured in the outer envelope type <see cref="TSODataServiceWrapperPDU"/>
        /// </summary>
        public uint TSODataDefinition_TypeStructID { get; set; }
        /// <summary>
        /// The number of fields contained in the <see cref="Fields"/> array. This is set by the protocol automatically.
        /// </summary>
        [TSOVoltronArrayLength(nameof(Fields))]
        public uint FieldsCount { get; set; }
        /// <summary>
        /// The fields contained within this TSODataDefinition structure
        /// </summary>
        public TSOValue_DataFieldValueWrapper[] Fields { get; set; } = new TSOValue_DataFieldValueWrapper[0];
    }

    /// <summary>
    /// This <see cref="TSOValueBase{T}"/> is used to represent a Property of a TSODataDefinition struct, whose value is itself a TSODataDefinition struct
    /// <para/>For example, <c>MyAvatar.Avatar_Appearance</c> (property) is of type <c>AvatarAppearance</c> which is a struct with three fields.
    /// </summary>
    [Serializable]
    internal class TSOValue_TSODataStructProperty : TSOValueBase<TSOValue_TSODataStructPropertyWrapper>
    {
        /// <summary>
        /// You can use this switch to force GetValue() to work without proper variable names, but this is VERY EASY to mess up - leave it false.
        /// </summary>
        public static bool ForceGetValue = false;

        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Property; // 0xA96E7E5B
        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;        

        public TSOValue_TSODataStructProperty() : base() { }
        public TSOValue_TSODataStructProperty(TSOValue_TSODataStructPropertyWrapper value) : base(value) { }

        public override object? GetValue()
        {
            if(!ForceGetValue)
                throw new InvalidOperationException($"You cannot call ITSOValue.GetValue() on a {GetType().Name}. You must use the overload that uses the TSODataService to get variable field names.");

            ExpandoObject obj = new();
            foreach(var field in Value.Fields)            
                obj.TryAdd(field.TSODataDefinitionFieldID.ToString(), field.TSOValue.GetValue());
            return obj;
        }
        /// <summary>
        /// This overload automatically dereferences the type names into 
        /// </summary>
        /// <param name="TSOData"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public TSODataRuntimeObject GetValue(TSODataDefinitionService TSOData)
        {            
            if(!TSOData.TryGetDataStruct(Value.TSODataDefinition_TypeStructID, out ITSODataStruct Struct))
                throw new Exception($"{Value.TSODataDefinition_TypeStructID} not found!");
            ExpandoObject obj = new();
            foreach (var field in Value.Fields)
                obj.TryAdd(Struct.GetFieldByID(field.TSODataDefinitionFieldID).NameString, field.TSOValue.GetValue());
            return new TSODataRuntimeObject(Struct, obj);
        }                
    }
}

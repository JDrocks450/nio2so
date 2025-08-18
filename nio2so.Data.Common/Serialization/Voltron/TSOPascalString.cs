using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Data.Common.Serialization.Voltron
{
    /// <summary>
    /// Wrapper class for a <see cref="string"/> with a <see cref="TSOVoltronString"/> attribute for use in primarily in arrays
    /// or to make your code clearer
    /// </summary>
    public class TSOPascalString
    {
        /// <summary>
        /// <inheritdoc cref="TSOVoltronValueTypes.Pascal"/>
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string Value { get; set; }
        /// <summary>
        /// <inheritdoc cref="Value"/>
        /// </summary>
        public TSOPascalString() : this("") { }
        /// <summary>
        /// <inheritdoc cref="Value"/>
        /// </summary>
        public TSOPascalString(string value)
        {
            Value = value;
        }
        public override string ToString() => $"(Pascal){Value}";
    }
}

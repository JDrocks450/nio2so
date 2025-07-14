using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Data.Common.Serialization.Voltron
{
    /// <summary>
    /// Wrapper class for a <see cref="String"/> with a <see cref="TSOVoltronString"/> attribute for use in primarily in arrays
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
    }

    /// <summary>
    /// A generic class that contains an array and a Numeric data type indicating the Length of the array preceding the array itself
    /// <code>[numeric] Length T[] Items</code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TSOVoltronBasicArray<Length, T>
    {
        /// <summary>
        /// The length of the <see cref="Array.Length"/> of the <see cref="Array"/> property
        /// </summary>
        [TSOVoltronSerializationAttributes.TSOVoltronArrayLength(nameof(Array))]        
        public Length Count { get; set; } = default(Length);
        /// <summary>
        /// The contents of the <see cref="TSOVoltronBasicArray{Length, T}"/>
        /// </summary>
        public T[] Array { get; set; } = new T[0];

        public TSOVoltronBasicArray() : base() { }
        /// <summary>
        /// Creates a new <see cref="TSOVoltronBasicArray{Length, T}"/> containing <paramref name="Items"/>
        /// </summary>
        /// <param name="Items"></param>
        public TSOVoltronBasicArray(params T[] Items) : this() {
            Array = Items;
        }
    }
}

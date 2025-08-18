namespace nio2so.Data.Common.Serialization.Voltron
{

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
        public Length Count { get; set; } = default;
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

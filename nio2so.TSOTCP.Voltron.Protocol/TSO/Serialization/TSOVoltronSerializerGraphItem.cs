using System.Collections;
using System.Reflection;

namespace nio2so.Voltron.Core.TSO.Serialization
{
    /// <summary>
    /// A serialization graph that graphs the objects serialized by the <see cref="TSOVoltronSerializerCore"/>
    /// <para/>Visually shows the serialization process for debugging/studying
    /// <para/>These can have <see cref="TSOVoltronSerializerGraphItem"/> children which would indicate a property 
    /// found on a larger enclosing type
    /// </summary>
    public record TSOVoltronSerializerGraphItem : IList<TSOVoltronSerializerGraphItem>
    {
        private readonly List<TSOVoltronSerializerGraphItem> _graph;
        private string? stringFormat;

        /// <summary>
        /// The name of the property being encoded, this can be blank.
        /// </summary>
        public string PropertyName { get; }
        public PropertyInfo? PropertyInfo { get; private set; } = null;
        public void AttachProperty(PropertyInfo info) => PropertyInfo = info;
        /// <summary>
        /// The type of object being encoded
        /// </summary>
        public Type SerializedType { get; }
        /// <summary>
        /// This encoded value, in a string format that adds clarity to the user viewing
        /// this graph in an editor
        /// </summary>
        public string SerializedValueStringFormat => (stringFormat ?? SerializedValue?.ToString()) ?? "no value";
        /// <summary>
        /// The object value being encoded
        /// </summary>
        public object SerializedValue { get; set; }
        /// <summary>
        /// The amount of bytes written in this serialization frame
        /// </summary>
        public long ByteLength { get; set; }

        /// <summary>
        /// Creates a new <see cref="TSOVoltronSerializerGraphItem"/>
        /// </summary>
        /// <param name="PropertyName">The name of the property being encoded, this can be blank.</param>
        /// <param name="SerializedType">The type of object being encoded</param>
        /// <param name="SerializedValue">The object value being encoded</param>
        /// <param name="ValueStringFormat">This encoded value, in a string format that adds clarity to the user viewing
        /// this graph in an editor</param>
        public TSOVoltronSerializerGraphItem(string? PropertyName, Type SerializedType, object SerializedValue, long byteLength, string? ValueStringFormat = null)
        {
            _graph = new();
            this.PropertyName = PropertyName ?? "";
            this.SerializedType = SerializedType;
            this.SerializedValue = SerializedValue;
            ByteLength = byteLength;
            stringFormat = ValueStringFormat;
        }
        /// <summary>
        /// <inheritdoc cref="TSOVoltronSerializerGraphItem(string, Type, object, string?)"/>
        /// </summary>
        /// <param name="Property"></param>
        /// <param name="SerializedType"></param>
        /// <param name="SerializedValue"></param>
        /// <param name="ValueStringFormat"></param>
        public TSOVoltronSerializerGraphItem(PropertyInfo? Property, Type SerializedType, object SerializedValue, long ByteLength, string? ValueStringFormat = null) : 
            this(Property?.Name, SerializedType, SerializedValue, ByteLength, ValueStringFormat)
        {
            if (Property != null)
                AttachProperty(Property);  
        }

        //**BELOW IS LIST FUNCTIONS**

        #region LIST FUNCTIONS
        public TSOVoltronSerializerGraphItem this[int index] { get => ((IList<TSOVoltronSerializerGraphItem>)_graph)[index]; set => ((IList<TSOVoltronSerializerGraphItem>)_graph)[index] = value; }

        public int Count => ((ICollection<TSOVoltronSerializerGraphItem>)_graph).Count;

        public bool IsReadOnly => ((ICollection<TSOVoltronSerializerGraphItem>)_graph).IsReadOnly;

        public void Add(TSOVoltronSerializerGraphItem item)
        {
            if (item == null)
                return;
            ((ICollection<TSOVoltronSerializerGraphItem>)_graph).Add(item);
        }

        public void Clear()
        {
            ((ICollection<TSOVoltronSerializerGraphItem>)_graph).Clear();
        }

        public bool Contains(TSOVoltronSerializerGraphItem item)
        {
            return ((ICollection<TSOVoltronSerializerGraphItem>)_graph).Contains(item);
        }

        public void CopyTo(TSOVoltronSerializerGraphItem[] array, int arrayIndex)
        {
            ((ICollection<TSOVoltronSerializerGraphItem>)_graph).CopyTo(array, arrayIndex);
        }

        public IEnumerator<TSOVoltronSerializerGraphItem> GetEnumerator()
        {
            return ((IEnumerable<TSOVoltronSerializerGraphItem>)_graph).GetEnumerator();
        }

        public int IndexOf(TSOVoltronSerializerGraphItem item)
        {
            return ((IList<TSOVoltronSerializerGraphItem>)_graph).IndexOf(item);
        }

        public void Insert(int index, TSOVoltronSerializerGraphItem item)
        {
            ((IList<TSOVoltronSerializerGraphItem>)_graph).Insert(index, item);
        }

        public bool Remove(TSOVoltronSerializerGraphItem item)
        {
            return ((ICollection<TSOVoltronSerializerGraphItem>)_graph).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<TSOVoltronSerializerGraphItem>)_graph).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_graph).GetEnumerator();
        }
        #endregion
    }
}

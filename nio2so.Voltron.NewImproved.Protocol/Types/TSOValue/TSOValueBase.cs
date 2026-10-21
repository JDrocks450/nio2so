namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    /// <summary>
    /// Represents a data type that contains a <see cref="TSO_NewImproved_TSOValueBase_CLSIDs"/> CLSID for a value type, followed by its typed-value
    /// </summary>
    public interface ITSOValue
    {
        /// <summary>
        /// This Class ID represents how the client will deserialize the Value in this <see cref="ITSOValue"/>
        /// </summary>
        TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; }
        /// <summary>
        /// Returns the value contained in this <see cref="ITSOValue"/> object
        /// </summary>
        /// <returns></returns>
        object? GetValue();
    }
    public interface ITSOValueVector : ITSOValue
    {
        IEnumerable<object>? GetItems() => (IEnumerable<object>)GetValue();
    }
    public interface ITSOValueMap : ITSOValue
    {
        IDictionary<int,object>? GetItems() => (IDictionary<int, object>?)GetValue();
    }

    /// <summary>
    /// <inheritdoc cref="ITSOValue"/>
    /// </summary>
    /// <typeparam name="T"><see cref="T"/> should be a <see cref="TSOVoltronSerializer"/> compatible type, and must be defined in <see cref="TSO_NewImproved_TSOValueBase_CLSIDs"/></typeparam>
    public abstract class TSOValueBase<T> : ITSOValue
    {
        protected TSOValueBase() { }
        protected TSOValueBase(T Value) : this()
        {
            this.Value = Value;
        }
        public abstract TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; }
        /// <summary>
        /// The value contained in this <see cref="TSOValueBase{T}"/> must match the type defined in <see cref="TypeClassID"/>
        /// </summary>
        public virtual T Value { get; set; } = default(T);
        public object? GetValue() => Value;
    }
}

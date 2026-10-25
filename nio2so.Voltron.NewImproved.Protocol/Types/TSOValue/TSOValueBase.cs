using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Util.Endian;
using nio2so.Voltron.Core.TSO.Serialization;
using System.Collections;
using System.Runtime.Serialization;

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
        /// <summary>
        /// <inheritdoc cref="TSOValueBase{T}.OnDeserialize(Stream)"/>
        /// <para/>See: <see cref="TSOValueBase{T}.OnDeserialize(Stream)"/>
        /// </summary>
        static TSOVoltron_OnDeserializeDelegate OnDeserialize => TSOValueBase<object>.OnDeserialize;
    }
    public interface ITSOValueVector : ITSOValue
    {
        ICollection GetItemCollection();
        int GetItemCount();
    }
    public interface ITSOValueMap : ITSOValue
    {

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
        public virtual object? GetValue() => Value;

        public override string ToString() => $"{TypeClassID} {GetType().Name} = ({typeof(T).Name}) {Value}";

        /// <summary>
        /// Custom deserialize method that automatically populates the embedded <see cref="ITSOValue"/> in the <paramref name="Stream"/>
        /// <para/>See: <see cref="TSOVoltron_OnDeserializeDelegate"/>
        /// </summary>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public static object OnDeserialize(Stream Stream)
        {
            EndianBitConverter conv = EndianBitConverter.Big;
            byte[] typeBytes = new byte[4];
            Stream.ReadExactly(typeBytes, 0, 4);
            TSO_NewImproved_TSOValueBase_CLSIDs clsID = (TSO_NewImproved_TSOValueBase_CLSIDs)conv.ToUInt32(typeBytes,0);

            Stream.Seek(-4, SeekOrigin.Current);

            Type valueType = TSOValueConverter.TypeFromCLSID(clsID);
            return TSOVoltronSerializer.Deserialize(Stream, valueType);
        }
    }
}

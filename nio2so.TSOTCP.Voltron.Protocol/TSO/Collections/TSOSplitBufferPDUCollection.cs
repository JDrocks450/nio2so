using nio2so.Voltron.Core.TSO.PDU;

namespace nio2so.Voltron.Core.TSO.Collections
{
    /// <summary>
    /// <see cref="DisposableCollection{T}"/> is a <see cref="List{T}"/> type that exposes the <see cref="Dispose"/> function of <see cref="IDisposable"/>    
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DisposableCollection<T> : List<T>, IDisposable where T : IDisposable
    {
        /// <summary>
        /// Disposes all objects in the list while simultaneously removing them.
        /// </summary>
        public void Dispose()
        {
            while (Count > 0)
            {
                var item = this[0];
                item.Dispose();
                RemoveAt(0);
            }
        }
    }
    /// <summary>
    /// <inheritdoc cref="TSOVoltronPacketCollectionBase{T}"/>
    /// </summary>
    public class TSOSplitBufferPDUCollection : TSOVoltronPacketCollectionBase<TSOSplitBufferPDUBase> { }
    /// <summary>
    /// A <see cref="DisposableCollection{T}"/> of <typeparamref name="T"/> instances. 
    /// <inheritdoc cref="DisposableCollection{T}"/>
    /// as <see cref="TSOVoltronPacket"/> is an <see cref="IDisposable"/> type
    /// </summary>
    public class TSOVoltronPacketCollectionBase<T> : DisposableCollection<T> where T : TSOVoltronPacket { }
    /// <summary>
    /// <inheritdoc cref="TSOVoltronPacketCollectionBase{T}"/>
    /// </summary>
    public class TSOVoltronPacketCollection : TSOVoltronPacketCollectionBase<TSOVoltronPacket> { }
}

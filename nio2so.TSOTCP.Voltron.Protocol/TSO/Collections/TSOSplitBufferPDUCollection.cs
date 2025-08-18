using nio2so.Voltron.Core.TSO.PDU;

namespace nio2so.Voltron.Core.TSO.Collections
{
    /// <summary>
    /// A collection of <see cref="TSOSplitBufferPDU"/> instances
    /// </summary>
    public class TSOSplitBufferPDUCollection : List<TSOVoltronPacket>, IDisposable
    {
        public void Dispose()
        {
            foreach (var item in this) item.Dispose();
            Clear();
        }
    }
}

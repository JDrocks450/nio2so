﻿using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Collections
{
    /// <summary>
    /// A collection of <see cref="TSOSplitBufferPDU"/> instances
    /// </summary>
    public class TSOSplitBufferPDUCollection : List<TSOSplitBufferPDU>, IDisposable
    {
        public void Dispose()
        {
            foreach (var item in this) item.Dispose();
            Clear();
        }
    }
}

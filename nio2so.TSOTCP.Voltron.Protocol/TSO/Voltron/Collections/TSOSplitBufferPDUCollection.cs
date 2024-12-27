using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Collections
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

using nio2so.DataService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol
{
    /// <summary>
    /// The <see cref="nio2soDataServiceClient"/> but for use in <see cref="ITSOServer"/> 
    /// <para/><inheritdoc cref="nio2soDataServiceClient"/>
    /// </summary>
    public class nio2soVoltronDataServiceClient : nio2soDataServiceClient, ITSOService
    {
        public nio2soVoltronDataServiceClient(Uri nio2soApiAddress) : this(new HttpClient(), nio2soApiAddress) { }
        /// <summary>
        /// <inheritdoc cref="nio2soDataServiceClient(HttpClient,Uri)"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="nio2soApiAddress"></param>
        public nio2soVoltronDataServiceClient(HttpClient client, Uri nio2soApiAddress) : base(client, nio2soApiAddress) { }

        public void Dispose()
        {
            Client.Dispose();
        }        
    }
}

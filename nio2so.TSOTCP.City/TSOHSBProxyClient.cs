using nio2so.Voltron.Core.TSO.Aries;
using QuazarAPI.Networking.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Server
{
    internal class TSOHSBProxyClient : QuazarAPI.Networking.Standard.QuazarClient<TSOTCPPacket>
    {
        public TSOHSBProxyClient() : base("TSO_HSB_Client_1", IPAddress.Loopback, 49100)
        {
            Strategy = ClientRecvStrategy.EVENT_BASED;
        }                
    }
}

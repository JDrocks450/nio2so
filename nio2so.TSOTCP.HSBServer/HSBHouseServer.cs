#define MAKEMANY
#undef MAKEMANY

using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.Voltron.Protocol;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using System.Net;

namespace nio2so.TSOTCP.HSBServer
{
    /// <summary>
    /// The <see cref="HSBHouseServer"/> is a The Sims Online Pre-Alpha Voltron Server analog.
    /// <para>It is designed to work with an unmodified copy of The Sims Online Pre-Alpha.</para>
    /// <para>It will by default host with a send/recv buffer allocated to be <see cref="TSO_Aries_SendRecvLimit"/></para>
    /// <para>To get the Client to connect to a <see cref="TSOCityServer"/>, ensure your TSOHTTPS server's ShardSelector points to the selected PORT,
    /// and you have modified your hosts file in System32/drivers/etc to include xo.max.ea.com</para>
    /// </summary>
    public class HSBHouseServer : TSOVoltronBasicServer
    {
        public HSBHouseServer(int port, IPAddress ListenIP = null) : base(nameof(HSBHouseServer), port, 5, ListenIP)
        {          
            CachePackets = false; // massive memory pit here if true
            DisposePacketOnSent = true; // no more memory leaks :)

            //**TELEMETRY
            if (TSOServerTelemetryServer.Global == null)
                Telemetry = new(Name, TSOVoltronConst.SysLogPath);
            else Telemetry = TSOServerTelemetryServer.Global;

            //**REGULATOR
            _regulatorManager = new(this);
            _regulatorManager.RegisterDefaultProtocols();
            _regulatorManager.RegisterProtocols(GetType().Assembly); // REGISTER CUSTOM PROTOCOL
        }

        public override void Start()
        {
            //Trigger tso factories to map using static constructor
            TSOFactoryBase.InitializeFactories();

            //HOOK EVENTS
            OnIncomingPacket += OnIncomingAriesFrameCallback;

            //START THE SERVER
            BeginListening();
        }

        public override void Stop()
        {

        }        
    }
}

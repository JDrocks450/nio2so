#define MAKEMANY
#undef MAKEMANY

using nio2so.Formats.BPS;
using nio2so.Formats.TSOData;
using nio2so.TSOTCP.Voltron.Protocol;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Services;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using System.Net;

namespace nio2so.TSOTCP.City.TSO
{

    /// <summary>
    /// The <see cref="TSOCityServer"/> is a The Sims Online Pre-Alpha Voltron Server analog.
    /// <para>It is designed to work with an unmodified copy of The Sims Online Pre-Alpha.</para>
    /// <para>It will by default host with a send/recv buffer allocated to be <see cref="TSOVoltronConst.TSOAriesClientBufferLength"/></para>
    /// <para>To get the Client to connect to a <see cref="TSOCityServer"/>, ensure your TSOHTTPS server's ShardSelector points to the selected PORT,
    /// and you have modified your hosts file in System32/drivers/etc in accordance with the niotso readme file.</para>
    /// </summary>
    public class TSOCityServer : TSOVoltronBasicServer
    {
        public TSOCityServer(int port, IPAddress ListenIP = null) : base(nameof(TSOCityServer), port, 5, ListenIP)
        {
            ClientBufferLength = TSOVoltronConst.TSOAriesClientBufferLength; // edit const to change this setting
            CachePackets = false; // massive memory pit here if true
            DisposePacketOnSent = true; // no more memory leaks :)

            //**TELEMETRY
            if (TSOServerTelemetryServer.Global == null)
                Telemetry = new(Name, TSOVoltronConst.SysLogPath);
            else Telemetry = TSOServerTelemetryServer.Global;

            //**REGULATOR
            _regulatorManager = new(this);
            _regulatorManager.RegisterDefaultProtocols(); // register all TSOTCP.Voltron protocols
            _regulatorManager.RegisterProtocols(GetType().Assembly); // register custom protocols
        }

        public override void Start()
        {
            //Trigger tso factories to map using static constructor
            TSOFactoryBase.InitializeFactories();

            //Startup Services
            Services.Register(new nio2soVoltronDataServiceClient(new(@"https://localhost:7071/api/"))); // REGISTER THE NIO2SO DATA SERVICE
            Services.Register(new nio2soClientSessionService()); // REGISTER THE CLIENT SESSION SERVICE

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

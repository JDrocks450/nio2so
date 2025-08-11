#define MAKEMANY
#undef MAKEMANY

using nio2so.DataService.Common.Types;
using nio2so.TSOTCP.Voltron.Protocol;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Services;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO;
using System.Configuration;
using System.Net;
using System.Net.Http.Json;

namespace nio2so.TSOTCP.Voltron.Server
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
        /// <summary>
        /// Uses the <see cref="LocalServerSettings.APIUrl"/> to download <see cref="VoltronServerSettings"/>
        /// </summary>
        /// <returns></returns>
        public static async Task<VoltronServerSettings?> DownloadSettings()
        {
            //API Url from local settings
            string APIUrl = LocalServerSettings.Default.APIUrl;
            using (HttpClient client = new HttpClient()
            {
                BaseAddress = new(APIUrl)
            })
            {
                string APIVoltronQuery = LocalServerSettings.Default.APIVoltronSettingsQuery;
                Console.WriteLine($"Downloading resource {APIUrl + APIVoltronQuery}");
                return (await client.GetFromJsonAsync<VoltronServerSettings>(APIVoltronQuery));
            }
        }

        /// <summary>
        /// Creates a new <see cref="TSOCityServer"/> with the specified <see cref="VoltronServerSettings"/>
        /// </summary>
        /// <param name="Settings"></param>
        public TSOCityServer(VoltronServerSettings Settings) : base(nameof(TSOCityServer), Settings)
        {
            ClientBufferLength = TSOVoltronConst.TSOAriesClientBufferLength; // edit const to change this setting
            CachePackets = false; // massive memory pit here if true
            DisposePacketOnSent = true; // no more memory leaks :)

            //**TELEMETRY
            if (TSOServerTelemetryServer.Global == null)
                Telemetry = new(Name, TSOVoltronConst.SysLogPath);
            else Telemetry = TSOServerTelemetryServer.Global;

            //**REGULATOR
            Regulators = new(this);
            Regulators.RegisterDefaultProtocols(); // register all TSOTCP.Voltron protocols
            Regulators.RegisterProtocols(GetType().Assembly); // register custom protocols
        }

        public override void Start()
        {
            //Trigger tso factories to map using static constructor
            TSOFactoryBase.InitializeFactories();

            //API Url from local settings
            string APIUrl = LocalServerSettings.Default.APIUrl;

            //Startup Services
            Services.Register(new nio2soVoltronDataServiceClient(new(APIUrl))); // REGISTER THE NIO2SO DATA SERVICE
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

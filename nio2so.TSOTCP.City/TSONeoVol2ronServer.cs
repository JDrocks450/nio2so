#define TSOPREALPHA
#define TSOPLAYTEST

//#undef TSOPREALPHA

#if TSOPREALPHA
#undef TSOPLAYTEST
#endif

using nio2so.DataService.Common.Types;
using nio2so.Voltron.Core;
using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using OpenSSL.X509;
using System.Net.Http.Json;

namespace nio2so.TSOTCP.Voltron.Server
{

    /// <summary>
    /// The <see cref="TSONeoVol2ronServer"/> is a The Sims Online Pre-Alpha Voltron Server analog.
    /// <para>It is designed to work with an unmodified copy of The Sims Online Pre-Alpha.</para>
    /// <para>It will by default host with a send/recv buffer allocated to be <see cref="TSOVoltronConst.TSOAriesClientBufferLength"/></para>
    /// <para>To get the Client to connect to a <see cref="TSONeoVol2ronServer"/>, ensure your TSOHTTPS server's ShardSelector points to the selected PORT,
    /// and you have modified your hosts file in System32/drivers/etc in accordance with the niotso readme file.</para>
    /// </summary>
    public class TSONeoVol2ronServer : TSOVoltronBasicServer
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

        static TSOLoggerServiceBase GetServerLogger(string SysLogPath = default)
        {
#if TSOPLAYTEST
            return new nio2so.Voltron.PlayTest.Protocol.TSOPlayTestLoggerService(SysLogPath);
#elif TSOPREALPHA
            return new nio2so.Voltron.PreAlpha.Protocol.TSOPreAlphaLoggerService(SysLogPath);
#endif
        }

        /// <summary>
        /// Creates a new <see cref="TSONeoVol2ronServer"/> with the specified <see cref="VoltronServerSettings"/>
        /// </summary>
        /// <param name="Settings"></param>
        public TSONeoVol2ronServer(VoltronServerSettings Settings, X509Certificate? ServerCertificate) : base(Settings.ShardName, Settings,
            GetServerLogger(TSOVoltronConst.SysLogPath), ServerCertificate)
        {
            //**REGULATOR
            Regulators.RegisterDefaultProtocols(); // register all Voltron.Core.TSO protocols (very few)
            
            // register The Sims Online targeting pack protocols (protocol assembly)
#if TSOPLAYTEST
            Regulators.RegisterProtocols(typeof(nio2so.Voltron.PlayTest.Protocol.Regulator.TSOProtocol).Assembly); 
#endif
#if TSOPREALPHA
            Regulators.RegisterProtocols(typeof(nio2so.Voltron.PreAlpha.Protocol.Regulator.TSOProtocol).Assembly); 
#endif
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
#if TSOPLAYTEST
            Services.Register(new nio2so.Voltron.PlayTest.Protocol.Services.TSOPlayTestPDUFactory()); // REGISTER THE TSOPLAYTEST PDU FACTORY
#endif
#if TSOPREALPHA
            Services.Register(new nio2so.Voltron.PreAlpha.Protocol.Services.TSOPreAlphaPDUFactory()); // REGISTER THE TSOPREALPHA PDU FACTORY
#endif

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

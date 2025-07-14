#define MAKEMANY
#undef MAKEMANY

using nio2so.Formats.BPS;
using nio2so.Formats.TSOData;
using nio2so.TSOTCP.Voltron.Protocol;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
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
            //PARSE TSO DATA DEFINITION DAT TO ROOT DIR
            string datLocation = "/packets/datadefinition.json";
            if (!File.Exists(datLocation))
            {
                var file = TSODataImporter.Import(@"E:\Games\TSO Pre-Alpha\TSO\TSOData_DataDefinition.dat");
                File.WriteAllText(datLocation, file.ToString());
            }

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

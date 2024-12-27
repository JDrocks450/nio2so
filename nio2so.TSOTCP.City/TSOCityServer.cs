#define MAKEMANY
#undef MAKEMANY

using nio2so.Formats.BPS;
using nio2so.Formats.FAR3;
using nio2so.Formats.Streams;
using nio2so.Formats.TSOData;
using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron.Collections;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.City.TSO.Voltron.Regulator;
using nio2so.TSOTCP.Voltron.Protocol;
using QuazarAPI;
using QuazarAPI.Networking.Standard;
using System.Net;
using System.Net.Sockets;

namespace nio2so.TSOTCP.City.TSO
{

    /// <summary>
    /// The <see cref="TSOCityServer"/> is a The Sims Online Pre-Alpha Voltron Server analog.
    /// <para>It is designed to work with an unmodified copy of The Sims Online Pre-Alpha.</para>
    /// <para>It will by default host with a send/recv buffer allocated to be <see cref="TSO_Aries_SendRecvLimit"/></para>
    /// <para>To get the Client to connect to a <see cref="TSOCityServer"/>, ensure your TSOHTTPS server's ShardSelector points to the selected PORT,
    /// and you have modified your hosts file in System32/drivers/etc to include xo.max.ea.com</para>
    /// </summary>
    public class TSOCityServer : TSOVoltronBasicServer
    {
        public TSOCityServer(int port, IPAddress ListenIP = null) : base(nameof(TSOCityServer), port, 5, ListenIP)
        {
            SendAmount = ReceiveAmount = TSO_Aries_SendRecvLimit; // 1MB by default           
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

            var bps = BPSFileInterpreter.Interpret(@"E:\Games\TSO Pre-Alpha\packetlog.bps");

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

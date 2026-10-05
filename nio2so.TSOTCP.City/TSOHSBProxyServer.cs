using nio2so.Data.Common.Testing;
using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.PreAlpha.Protocol;
using QuazarAPI.Networking.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Server
{
    internal class TSOHSBProxyServer : QuazarAPI.Networking.Standard.QuazarServer<TSOTCPPacket>
    {
        byte[] aries_infoPacket =
        {
            0x39, 0x30, 0x30, 0x30,
            0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            // "5.5.0.4o"
            0x35, 0x2E, 0x35, 0x2E, 0x30, 0x2E, 0x34, 0x6F,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,

            0x30, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            // "VoltronAuthserv"
            0x56, 0x6F, 0x6C, 0x74, 0x72, 0x6F, 0x6E, 0x41,
            0x75, 0x74, 0x68, 0x73, 0x65, 0x72, 0x76,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,

            0x01, 0x00,
            0x39, 0x41, 0x00, 0x00,
            0x1B, 0x00,

            // "aries_debug:crap:RoomServer"
            0x61, 0x72, 0x69, 0x65, 0x73, 0x5F, 0x64, 0x65,
            0x62, 0x75, 0x67, 0x3A, 0x63, 0x72, 0x61, 0x70,
            0x3A, 0x52, 0x6F, 0x6F, 0x6D, 0x53, 0x65, 0x72,
            0x76, 0x65, 0x72, 0x00
        };

        /// <summary>
        /// Client used to forward packets to Voltron.
        /// </summary>
        TSOHSBProxyClient _voltronClient;
        uint HSB_ID = 0;
        bool ACTIVATED = false;
        private TSOPDUFactoryServiceBase _PDUFactory => Server.Services.Get<TSOPDUFactoryServiceBase>();
        public TSONeoVol2ronServer Server { get; }

        public TSOHSBProxyServer(TSONeoVol2ronServer Server, string Name = "TSO_HSB_PROXY", int Port = 49101) : base(Name, Port)
        {
            this.Server = Server;
        }

        public override async void Start()
        {
            CONSOLE_LOG("\n[!] Attempting to connect to Voltron...");
            bool connected = await CreateClientAsync();
            if (connected)
            {
                CONSOLE_LOG("Voltron is ONLINE! Creating HSB Proxy server now...");

                OnIncomingPacket += TSOHSBProxyServer_OnIncomingPacket;
                _voltronClient.OnPacketReceived += Voltron_PacketReceived;

                BeginListening();

                CONSOLE_LOG("HSB Proxy is ONLINE!");
            }
        }        

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);
            HSB_ID = ID;
            ACTIVATED = false;
            
            //_voltronClient.SendPacket(new TSOTCPPacket(TSOAriesPacketTypes.Client_SessionInfoResponse,0,aries_infoPacket));
            ActivateHSB(ID);
        }

        /// <summary>
        /// Waits for a Magic Packet to get the HSB to advance to asking for which Lot to host.
        /// </summary>
        void ActivateHSB(uint QuazarID)
        {
            //ARIES_GETCLIENTINFO -- needed to get the TSOClient to actually send anything.
            Send(QuazarID, new TSOTCPPacket(TSOAriesPacketTypes.ClientSessionInfo, 0, 0));
            ACTIVATED = true;
        }

        /// <summary>
        /// Packet from HSB -> PROXY_SERVER -> VOLTRON
        /// </summary>
        /// <param name="ClientID"></param>
        /// <param name="Packet"></param>
        private void TSOHSBProxyServer_OnIncomingPacket(uint ClientID, TSOTCPPacket Packet)
        {
            if (TestingConstraints.VerboseLogging)
                CONSOLE_LOG("[HSB -> PROXY] " + Packet);

            _ = _voltronClient.SendPacket(Packet);
        }

        /// <summary>
        /// Packet from VOLTRON -> PROXY_SERVER -> HSB
        /// </summary>
        private void Voltron_PacketReceived(object? sender, QEventArgs<TSOTCPPacket> e)
        {
            if (e.Data == null)
                return;

            if (!ACTIVATED)
            {
                try
                {
                    var packets = _PDUFactory.CreatePacketObjectsFromAriesPacket(e.Data);
                    if (packets.Any(x => x.VoltronPacketType == (ushort)TSO_PreAlpha_VoltronPacketTypes.ROOMSERVER_INITIALIZED_PDU))
                        ActivateHSB(HSB_ID);
                }
                catch
                {

                }
            }

            if (TestingConstraints.VerboseLogging)
                CONSOLE_LOG("[PROXY -> HSB] " + e.Data);

            if (ACTIVATED)
                Send(HSB_ID, e.Data);
        }

        private async Task<bool> CreateClientAsync()
        {
            _voltronClient = new TSOHSBProxyClient();
            await _voltronClient.Connect();
            return _voltronClient.IsConnected;
        }        

        private void CONSOLE_LOG(string message) => Console.WriteLine("DEBUG_HSBTEST: " + message);


        public override void Stop()
        {
            
        }
    }
}

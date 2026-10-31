using nio2so.Data.Common.Testing;
using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.PreAlpha.Protocol;
using OpenSSL.X509;
using QuazarAPI.Networking.Standard;
using System.Drawing;
using System.Net;
using System.Net.Sockets;

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
        //private nio2so.Voltron.PreAlpha.Protocol.Regulator.SplitBufferPDUProtocol _splitBufferInterface;

        public TSONeoVol2ronServer Server { get; }

        /// <summary>
        /// Creates a new <see cref="TSOHSBProxyServer"/> with the given parent server <paramref name="Server"/>
        /// <para/>For Pre-Alpha: NO SSL certificate should be provided. For Play-Test and onward, you must supply ANY valid certificate
        /// for the <see cref="TSONeoVol2ronServer"/> and this <see cref="TSOHSBProxyServer"/> (and by extension, the <see cref="TSOHSBProxyClient"/> - but this is
        /// done for you)
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="Certificate"></param>
        /// <param name="Name"></param>
        /// <param name="Port"></param>
        public TSOHSBProxyServer(bool IsPreAlpha, TSONeoVol2ronServer Server, X509Certificate? Certificate = null, string Name = "TSO_HSB_PROXY", int Port = 49101) : base(new QuazarServerSettings(Name, IPAddress.Loopback, Port, Certificate))
        {
            this.Server = Server;
            if (!IsPreAlpha && Certificate == null)
                throw new ArgumentException("For Play-Test and later, you HAVE to give a certificate.");
        }

        public override async void Start()
        {
            CONSOLE_LOG($"\n[!] Attempting to connect to Voltron [SSL?: {UsingSSL}]...");
            bool connected = await CreateClientAsync();
            if (connected)
            {
                CONSOLE_LOG("Creating HSB Proxy server now...");

                OnIncomingPacket += TSOHSBProxyServer_OnIncomingPacket;                
                BeginListening();

                CONSOLE_LOG("HSB Proxy is online and connected to Voltron!");                
            }
        }        

        protected override async void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);

            CONSOLE_LOG($"HSB Simulator Client has arrived, authenticating with Voltron [SSL?: {UsingSSL}]...");

            //just in-case
            DeactivateHSB();

            HSB_ID = ID;
            
            await _voltronClient.SendPacket(new TSOTCPPacket(TSOAriesPacketTypes.Client_SessionInfoResponse,0,aries_infoPacket));
            uint avatarID = 90001;
            CONSOLE_LOG("Authentication sent for HSB Avatar ID: " + avatarID);

            if (UsingSSL)
                ActivateHSB(ID);
        }

        protected override async void OnClientDisconnect(uint ID)
        {
            base.OnClientDisconnect(ID);

            CONSOLE_LOG($"HSB Client on connection: {ID} disconnected, resetting connection (Read PDU Mode: ON)....");

            DeactivateHSB();

            DestroyClient();
            await CreateClientAsync();
        }

        /// <summary>
        /// Waits for a Magic Packet to get the HSB to advance to asking for which Lot to host.
        /// </summary>
        void ActivateHSB(uint QuazarID)
        {
            //read pdu mode off is raw data mode: OnDataReceived()
            CONSOLE_LOG("HSB Simulator Client has received it's awake signal, TSOClient is entering Voltron (Read PDU Mode: OFF)...");
            _voltronClient.EventParameters = new() { OnPacketReceivedEnabled = false };

            //ARIES_GETCLIENTINFO -- needed to get the TSOClient to actually send anything.
            //Send(QuazarID, new TSOTCPPacket(TSOAriesPacketTypes.ClientSessionInfo, 0, 0));
            ACTIVATED = true;
        }

        /// <summary>
        /// Waits for a Magic Packet to get the HSB to advance to asking for which Lot to host.
        /// </summary>
        void DeactivateHSB()
        {
            HSB_ID = 0;
            ACTIVATED = false;

            _voltronClient.EventParameters = new() { OnPacketReceivedEnabled = true };
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

            IEnumerable<TSOVoltronPacket>? packets = null; 

            if (!ACTIVATED)
            {
                try
                {
                    packets = _PDUFactory.CreatePacketObjectsFromAriesPacket(e.Data);
                    if (packets.Any(x => x.VoltronPacketType == (ushort)TSO_PreAlpha_VoltronPacketTypes.ROOMSERVER_INITIALIZED_PDU))
                        ActivateHSB(HSB_ID);
                    //** Activation switches to raw data OnDataReceived event exclusively -- this method no longer powers client function to ensure less error arise
                }
                catch (Exception ex)
                {
                    CONSOLE_LOG("An error has occured: " + ex);
                }
            }

            if (TestingConstraints.VerboseLogging)
            {
                if (packets == null)
                    packets = _PDUFactory.CreatePacketObjectsFromAriesPacket(e.Data);
                foreach (var packet in packets)
                    CONSOLE_LOG("[PROXY -> HSB] -- " + packet.ToString());
            }
        }        

        private void VoltronClient_OnDataReceived(object? sender, QEventArgs<byte[]> e)
        { // forward raw data as it comes in, no need to check packets.
            if (e.Data.Length == 0)
                return;

            if (!WaitForAriesFrame(e.Data, out int waitingAmount, out byte[]? client_buffer))
            {
                CONSOLE_LOG($"[PROXY -> HSB] -- Waiting for complete frame: {aries_stream.Length} of {waitingAmount} bytes.");
                return;
            }

            if (ACTIVATED)
            {
                Send(HSB_ID, client_buffer);
                CONSOLE_LOG("[PROXY -> HSB] -- Sent: " + client_buffer.Length + " bytes.");
            }
        }

        //**aries code
        bool aries_packet_waiting = false;
        uint totalPacketSize = 0;
        MemoryStream aries_stream;
        /// <summary>
        /// This function will wait for a full aries frame to be received before writing it to the outgoing data stream
        /// <para/>Returns true when <paramref name="OutgoingData"/> is ready to send, false when it isn't and we're waiting for more.
        /// </summary>
        /// <param name="IncomingData"></param>
        /// <param name="OutgoingData"></param>
        /// <returns></returns>
        private bool WaitForAriesFrame(byte[] IncomingData, out int WaitingAmount, out byte[]? OutgoingData)
        {
            OutgoingData = null;
            WaitingAmount = (int)totalPacketSize;

            if (aries_packet_waiting)
            {
                aries_stream.Write(IncomingData);
                if (aries_stream.Length >= totalPacketSize)
                {
                    aries_packet_waiting = false;
                    totalPacketSize = 0;
                    OutgoingData = aries_stream.ToArray();
                    return true;
                }
            }
            else
            {
                TSOTCPPacket.GetAriesHeader(IncomingData, out _, out _, out uint size);
                WaitingAmount = (int)size;
                if (size > (IncomingData.Length - TSOTCPPacket.ARIES_FRAME_HEADER_LEN)) // split
                {
                    aries_stream = new MemoryStream();
                    aries_stream.Write(IncomingData);
                    aries_packet_waiting = true;
                    totalPacketSize = size;
                }
            }
            if (aries_packet_waiting)
                return false;

            WaitingAmount = IncomingData.Length;
            OutgoingData = IncomingData;
            return true;
        }

        private async Task<bool> CreateClientAsync()
        {
            _voltronClient = new TSOHSBProxyClient();
            await _voltronClient.Connect(Settings.ServerCertificate);
            if (_voltronClient.IsConnected)
            {
                _voltronClient.OnPacketReceived += Voltron_PacketReceived;
                _voltronClient.OnDataReceived += VoltronClient_OnDataReceived;
                CONSOLE_LOG($"Voltron connection has been established! [SSL?: {UsingSSL}]");
            }
            return _voltronClient.IsConnected;
        }   
        
        void DestroyClient()
        {
            _voltronClient?.Dispose();
            _voltronClient = null;
        }

        private void CONSOLE_LOG(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("***\n\nDEBUG_HSBTEST: " + message + "\n\n***");
        }


        public override void Stop()
        {
            
        }
    }
}

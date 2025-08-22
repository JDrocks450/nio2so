using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types;
using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.PDU;
using nio2so.Voltron.Core.TSO.Regulator;
using OpenSSL.X509;
using QuazarAPI.Networking.Standard;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace nio2so.Voltron.Core
{
    /// <summary>
    /// A basic Voltron Server that handles the <see cref="TSOTCPPacket"/>s and <see cref="TSOVoltronPacket"/>s for The Sims Online Pre-Alpha.
    /// </summary>
    public abstract class TSOVoltronBasicServer : QuazarServer<TSOTCPPacket>, ITSOServer
    {
        const bool DefaultRunningState = true;

        /// <summary>
        /// The size of the ClientBuffer and SendBuffer for incoming and outgoing data
        /// </summary>
        public ushort ClientBufferLength => (ushort)Math.Max(SendAmount,ReceiveAmount);

        /// <summary>
        /// Manages all <see cref="ITSOProtocolRegulator"/>s registered to this server -- you can add custom regulators to the <see cref="TSOVoltronBasicServer"/> using this property.
        /// </summary>
        public TSORegulatorManager Regulators { get; }
        /// <summary>
        /// <inheritdoc cref="ITSOService"/>
        /// </summary>
        public TSOServerServiceManager Services { get; }
        public TSOLoggerServiceBase Logger { get; }

        /// <summary>
        /// True to pause the processing of any Voltron Packets until this is false
        /// </summary>
        public bool IsRunning
        {
            get => _running;
            set
            {
                if (value) ServerPauseEvent.Set();
                else ServerPauseEvent.Reset();
            }
        }
        /// <summary>
        /// Gets the assembly info of the nio2so Voltron Protocol used by this server.
        /// </summary>
        public static Assembly nio2soVoltronProtocolAssembly => typeof(TSOVoltronBasicServer).Assembly;
        /// <summary>
        /// Gets the version of the nio2so Voltron Protocol used by this server.
        /// </summary>
        public static FileVersionInfo nio2soVoltronProtocolVersion => FileVersionInfo.GetVersionInfo(nio2soVoltronProtocolAssembly.Location);
        /// <summary>
        /// String form of the <see cref="QuazarServer{T}.QuaZarProtocolVersion"/> and <see cref="nio2soVoltronProtocolVersion"/> used by this server.
        /// </summary>
        public static string ServerVersionInfoString => $"==={nameof(nio2soVoltronProtocolVersion)}===\n {nio2soVoltronProtocolVersion}\n" +
                                                        $"==={nameof(QuaZarProtocolVersion)}===\n {QuaZarProtocolVersion}";

        protected ManualResetEvent ServerPauseEvent = new(DefaultRunningState);
        protected bool _running = DefaultRunningState;

        /// <summary>
        /// Creates a new <see cref="TSOVoltronBasicServer"/> with the specified name and <see cref="VoltronServerSettings"/>.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Settings"></param>
        protected TSOVoltronBasicServer(string Name, VoltronServerSettings Settings, TSOLoggerServiceBase TelemetryServer, X509Certificate? ServerCertificate) :
            base(new(Name, Settings.ServerIPAddress == "localhost" ? IPAddress.Loopback : IPAddress.Parse(Settings.ServerIPAddress), Settings.ServerPort, ServerCertificate, Settings.MaxConcurrentConnections)
            {
                CachePackets = false, // no memory leaks please
                DisposePacketOnSent = true, // no memory leaks please
                ReceiveAmount = Settings.TSOAriesClientBufferLength,
                SendAmount = Settings.TSOAriesClientBufferLength
            })
        {
            Regulators = new(this);
            Services = new(this);
            Logger = TelemetryServer;
            Services.Register(TelemetryServer);
        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);

            //ARIES_GETCLIENTINFO
            Send(ID, new TSOTCPPacket(TSOAriesPacketTypes.ClientSessionInfo, 0, 0));
        }

        protected override void OnClientDisconnect(uint ID)
        {
            base.OnClientDisconnect(ID);

            //Update VoltronDMS
            Regulators.Get<IDMSProtocol>().ON_DISCONNECT(ID);
        }

        protected void OnIncomingAriesFrameCallback(uint ID, TSOTCPPacket Data)
        {
            ServerPauseEvent.WaitOne(); // FORCE SINGLE THREADED FOR RIGHT NOW
            //ServerPauseEvent.Reset();

            //**Telemetry
            Logger.OnAriesPacket(NetworkTrafficDirections.INBOUND, DateTime.Now, Data);

            switch (Data.PacketType)
            {
                case (uint)TSOAriesPacketTypes.Client_SessionInfoResponse:
                    {
                        //**Client is logging into Voltron!
                        var sessionData = TSOAriesClientSessionInfo.FromPacket(Data);
                        TSOTCPPacket.WriteAllPacketsToDisk([Data]);

                        if (!uint.TryParse(sessionData.User, out uint AvatarID))
                        {
                            Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Errors, Name, $"AvatarID {sessionData.User} is unknown and was disconnected."));
                            Disconnect(ID);
                            break;
                        }
                        Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message, Name, $"AvatarID {sessionData.User} is logging into Voltron on nio2so!"));

                        //GET THE VOLTRON DMS REGULATOR
                        var dmsProtocol = Regulators.Get<IDMSProtocol>();

                        //HOST_ONLINE_PDU
                        SubmitAriesFrame(ID, dmsProtocol.GET_HOST_ONLINE(ClientBufferLength, "badword"));

                        //attempt to find the client session service   
                        if (!Services.TryGet<nio2soClientSessionService>(out var sessionService))
                        { // not added to this server, cannot continue.
                            Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Errors, Name, $"Could not find the ClientSessionService! Disconnecting..."));
                            Disconnect(ID);
                            break;
                        }

                        //Check if loading into CAS
                        if (AvatarID == 0)
                        { // loading into CAS
                            sessionService.AddClientInCAS(ID);
                            Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Warnings, Name, $"UserLogin(): User entering CAS."));
                            break;
                        }

                        //UPDATE_PLAYER_PDU
                        //Download player avatar name from data service.
                        try
                        {
                            //download the player's name from data service.
                            var UpdatePlayerPDU = dmsProtocol.GET_UPDATE_PLAYER(AvatarID, out string AvatarName);
                            if (UpdatePlayerPDU == null)
                                throw new NullReferenceException(nameof(UpdatePlayerPDU));
                            //add this connection to the session service .. this identifies the player by incoming PDU
                            sessionService.AddClient(ID, new(AvatarID, AvatarName));
                            //send the update player PDU to the client
                            SubmitAriesFrame(ID, UpdatePlayerPDU);
                            Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Warnings, Name, $"UserLogin(): AvatarID: {AvatarID} AvatarName: {AvatarName} (downloaded from nio2so)"));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Errors, Name, ex.Message));
                            Disconnect(ID);
                            break;
                        }
                    }
                    break;
                case (uint)TSOAriesPacketTypes.Voltron:
                    //**all code contained here should be consider thread independent (no references to shared resources that aren't thread-safe)                                        
                    BeginVoltronFrame_Threaded(ID, Data);
                    break;
            }
            ServerPauseEvent.Set();
            Data.Dispose();
        }

        private void BeginVoltronFrame_Threaded(uint QuazarID, TSOTCPPacket IncomingAriesPacket)
        {
            uint ID = QuazarID;

            List<TSOVoltronPacket> _VoltronBacklog = new();
            List<TSOVoltronPacket> packetSendQueue = new List<TSOVoltronPacket>();

            uint? disconnectingID = null;

            // Sends the current packetSendQueue in a TSOAriesPacket
            void CloseAriesFrame(uint ID)
            {
                while (packetSendQueue.Any())
                {
                    var voltronPacket = packetSendQueue.First();
                    packetSendQueue.Remove(voltronPacket);

                    SubmitAriesFrame(ID, voltronPacket);
                }
                packetSendQueue.Clear();
                _VoltronBacklog.Clear();

                if (disconnectingID.HasValue)
                    Disconnect(disconnectingID.Value,null);
            }

            //refer to client session service to get this client's connection
            nio2soClientSessionService sessionService = Services.Get<nio2soClientSessionService>();
            if (!sessionService.TryIdentify(ID, out _)) // this ensures services can identify this client if needed
            { // is this client in cas?
                if (!sessionService.IsInCAS(ID))
                { // not in CAS either, disconnect.
                    throw new InvalidDataException($"Could not identify client: {ID}");
                    Disconnect(ID, null); // disconnect this unknown client
                }
                //is in CAS, proceed.
            }

            //GET VOLTRON PACKETS OUT OF DATA BUFFER
            var factoryService = Services.Get<TSOPDUFactoryServiceBase>();
            var packets = factoryService.CreatePacketObjectsFromAriesPacket(IncomingAriesPacket);
            _VoltronBacklog.AddRange(packets); // Enqueue all incoming PDUs
            while (_VoltronBacklog.Count > 0)
            { // Work through all incoming PDUs ... list can change foreach would be unsafe here                           
                var packet = _VoltronBacklog[0];
                _VoltronBacklog.RemoveAt(0); // pop

                Debug_LogSendPDU(ID, packet, NetworkTrafficDirections.INBOUND); // DEBUG LOGGING

                try
                {
                    bool _handled = false; // DICTATES WHETHER HANDLER WAS SUCCESSFUL
                    var returnPackets = OnIncomingVoltronPacket(ref _VoltronBacklog, ID, packet, ref _handled, out disconnectingID); // handle pdu
                    if (!_handled)
                    { // HANDLER FAILED
                        Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Warnings, "Voltron Handler",
                            $"Handling {packet.GetVoltronPacketTypeName(this)} Voltron packet was not successful."));
                        continue;
                    }
                    packetSendQueue.AddRange(returnPackets); // ADD TO SEND QUEUE
                }
                catch (Exception ex)
                {
                    string errorPath = Path.Combine(TestingConstraints.WorkspaceDirectory, "tsotcppackets",
                        $"errorPacket [{factoryService.GetVoltronPacketTypeName(packet.VoltronPacketType)}].dat");
                    Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Errors, "Voltron Handler",
                            $"Handling the {packet.GetVoltronPacketTypeName(this)} Voltron packet resulted in an error. Dumped to: {errorPath} \n" +
                        $"{ex}"));
                    //log error packet
                    File.WriteAllBytes(errorPath,packet.Body);
                }
            }
            CloseAriesFrame(ID);
        }

        /// <summary>
        /// When a packet is received and parsed out, this function is called to enable the server to respond accordingly. 
        /// <para>You can send none, one, or many voltron packets.</para>
        /// <para>These packets will be automatically sent back to the client on the next available aries frame.</para>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Packet"></param>
        /// <param name="Handled"></param>
        /// <returns></returns>
        private IEnumerable<TSOVoltronPacket> OnIncomingVoltronPacket(ref List<TSOVoltronPacket> VoltronBacklog, uint ID, TSOVoltronPacket Packet, ref bool Handled, out uint? disconnectingID)
        {
            disconnectingID = null;

            List<TSOVoltronPacket> packets = new();
            void defaultSend(TSOVoltronPacket packetToSend)
            {
                Debug_LogSendPDU(ID, packetToSend, NetworkTrafficDirections.CREATED);
                packets.Add(packetToSend);
            }

            // TRY USING THE REGULATOR MANAGER TO HANDLE THIS PACKET
            if (!Handled)
            { // INVOKE THE SERVER-SIDE REGULATOR MANAGER
                if (Regulators.HandleIncomingPDU(Packet, out TSOProtocolRegulatorResponse Response))
                { // REGULATOR HANDLED
                    if (Response != null)
                    {
                        disconnectingID = Response.DisconnectingID;
                        if (Response.InsertionPackets != null) // ADD INSERTIONS TO BACKLOG
                        {
                            foreach (var packet in Response.InsertionPackets)
                                VoltronBacklog.Insert(0, packet);
                        }
                        if (Response.EnqueuePackets != null) // ADD INSERTIONS TO BACKLOG
                        {
                            foreach (var packet in Response.EnqueuePackets)
                                VoltronBacklog.Add(packet);
                        }
                        if (Response.ResponsePackets != null) // ADD RESPONSE TO SENDQUEUE
                        {
                            foreach (var packet in Response.ResponsePackets)
                                defaultSend(packet);
                        }
                        if (Response.SessionPackets != null) // ADD SESSION TO SENDQUEUE
                        {
                            foreach (var packet in Response.SessionPackets)
                            {
                                uint sendID = packet.Session;
                                Debug_LogSendPDU(sendID, packet.Packet, NetworkTrafficDirections.CREATED);
                                SubmitAriesFrame(sendID, packet.Packet);
                            }
                        }
                        Handled = true;
                    }
                }
            }
            return packets;
        }

        /// <summary>
        /// Submits the <see cref="TSOVoltronPacket"/> to be sent to the remote connection <paramref name="ID"/> 
        /// <para/>This will call <see cref="SplitLargePDU(TSOVoltronPacket)"/> on a larger <see cref="TSOVoltronPacket"/> size than 
        /// <see cref="ClientBufferLength"/>!
        /// <para/>Please note: TCPQuaZar also will automatically slice the data buffer of a <see cref="TSOTCPPacket"/> into multiple buffers if this is an overage.. but don't rely on this.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="voltronPacket"></param>
        private void SubmitAriesFrame(uint ID, TSOVoltronPacket voltronPacket)
        {
            IEnumerable<TSOVoltronPacket> largePDUs = SplitLargePDU(voltronPacket);
            if (largePDUs.Count() > 1) // log console on auto TSOSplitBuffers            
                Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message, Name,
                    $"***\n" +
                    $"\n" +
                    $"Split {voltronPacket.ToShortString()} into {largePDUs.Count()} {largePDUs.First().GetType().Name}... " +
                    $"({largePDUs.Count()} x {ClientBufferLength}(max) = {largePDUs.Sum(x => x.BodyLength)} bytes)\n" +
                    $"\n" +
                    $"***"));            
            foreach (var splitPDU in largePDUs)
            {
                TSOTCPPacket ariesPacket = TSOVoltronPacket.MakeVoltronAriesPacket(splitPDU);
                if (ariesPacket.BodyLength > ClientBufferLength)
                { // ERROR! Size too large!
                    if (TestingConstraints.SplitBuffersPDUEnabled)
                        throw new OverflowException("The size of the last TSOTCPPacket was way too large.");
                }
                Send(ID, ariesPacket);
                Logger.OnAriesPacket(NetworkTrafficDirections.OUTBOUND, DateTime.Now, ariesPacket, voltronPacket); // DEBUG LOGGING for ARIES packets
            }                        
            Logger.OnVoltronPacket(NetworkTrafficDirections.OUTBOUND, DateTime.Now, voltronPacket); // DEBUG LOGGING FOR VOLTRON PDUs
        }

        /// <summary>
        /// Helper function to automatically create a <see cref="TSOSplitBufferPDU"/> for largely sized networked data
        /// <para/> See: <see cref="TestingConstraints.SplitBuffersPDUEnabled"/> and <see cref="TSOSplitBufferPDU.STANDARD_CHUNK_SIZE"/>
        /// </summary>
        /// <param name="DBWrapper"></param>
        /// <returns></returns>
        protected IEnumerable<TSOVoltronPacket> SplitLargePDU(TSOVoltronPacket DBWrapper)
        {
            List<TSOVoltronPacket> packets = new();
            //ensure to remove the ARIES header size from the split size to ensure we don't spill over            
            uint splitSize = ClientBufferLength != 0 ? (uint)ClientBufferLength - TSOTCPPacket.ARIES_FRAME_HEADER_LEN : TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;
            if (DBWrapper.BodyLength > splitSize && TestingConstraints.SplitBuffersPDUEnabled)
                // ONLY USE ITSOSPLITBUFFERPDU ON A VOLTRON PACKET OBJECT
                packets.AddRange(Services.Get<TSOPDUFactoryServiceBase>().CreateSplitBufferPacketsFromPDU(DBWrapper, splitSize).Cast<TSOVoltronPacket>());
            else
                packets.Add(DBWrapper);
            return packets;
        }

        /// <summary>
        /// Logs to the debug output stream the packet's information so it can be assessed when debugging.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PDU"></param>
        /// <param name="Verb"></param>
        private void Debug_LogSendPDU(uint ID, TSOVoltronPacket PDU, NetworkTrafficDirections Direction) =>
            Logger.OnVoltronPacket(Direction, DateTime.Now, PDU, ID);
    }
}

using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Aries;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using QuazarAPI;
using QuazarAPI.Networking.Standard;
using System.Net;
using System.Net.Sockets;

namespace nio2so.TSOTCP.Voltron.Protocol
{
    public abstract class TSOVoltronBasicServer : QuazarServer<TSOTCPPacket>, ITSOServer
    {
        const bool DefaultRunningState = true;

        /// <summary>
        /// The size of the ClientBuffer and SendBuffer for incoming and outgoing data
        /// </summary>
        public ushort ClientBufferLength { get; set; } = TSOVoltronConst.TSOAriesClientBufferLength;
        public override int ReceiveAmount => ClientBufferLength;
        public override int SendAmount => ClientBufferLength;


        protected List<TSOVoltronPacket> _VoltronBacklog = new();
        List<TSOVoltronPacket> packetSendQueue = new List<TSOVoltronPacket>();

        protected TSORegulatorManager _regulatorManager;

        public TSOServerTelemetryServer Telemetry { get; protected set; }

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
        protected ManualResetEvent ServerPauseEvent = new(DefaultRunningState);
        protected bool _running = DefaultRunningState;

        protected TSOVoltronBasicServer(string name, int port, uint backlog = 1, IPAddress ListenIP = null) :
            base(name, port, backlog, ListenIP) { }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);

            //ARIES_GETCLIENTINFO
            Send(ID, new TSOTCPPacket(TSOAriesPacketTypes.ClientSessionInfo, 0, 0));
        }

        protected void OnIncomingAriesFrameCallback(uint ID, TSOTCPPacket Data)
        {
            //**Telemetry
            Telemetry.OnAriesPacket(NetworkTrafficDirections.INBOUND, DateTime.Now, Data);

            switch (Data.PacketType)
            {
                case (uint)TSOAriesPacketTypes.Client_SessionInfoResponse:
                    {
                        //**Client is logging into Voltron!
                        var sessionData = TSOAriesClientSessionInfo.FromPacket(Data);
                        TSOTCPPacket.WriteAllPacketsToDisk([Data]);
                        Telemetry.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Discoveries, Name, $"User {sessionData.User} is logging into Voltron on nio2so!"));            

                        //HOST_ONLINE_PDU
                        SubmitAriesFrame(ID, new TSOHostOnlinePDU(ClientBufferLength, "badword"));
                        //UPDATE_PLAYER_PDU
                        SubmitAriesFrame(ID, new TSOUpdatePlayerPDU(new TSOAriesIDStruct(uint.Parse(sessionData.User), TestingConstraints.MyAvatarName)));
                    }
                    break;
                case (uint)TSOAriesPacketTypes.Voltron:
                    {
                        //GET VOLTRON PACKETS OUT OF DATA BUFFER
                        var packets = TSOVoltronPacket.ParseAllPackets(Data);
                        _VoltronBacklog.AddRange(packets); // Enqueue all incoming PDUs
                        while (_VoltronBacklog.Count > 0)
                        { // Work through all incoming PDUs
                            ServerPauseEvent.WaitOne();

                            var packet = _VoltronBacklog[0];
                            _VoltronBacklog.RemoveAt(0);

                            Debug_LogSendPDU(ID, packet, NetworkTrafficDirections.INBOUND); // DEBUG LOGGING

                            try
                            {
                                bool _handled = false; // DICTATES WHETHER HANDLER WAS SUCCESSFUL
                                var returnPackets = OnIncomingVoltronPacket(ID, packet, ref _handled);
                                if (!_handled)
                                { // HANDLER FAILED
                                    Telemetry.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Warnings, "Voltron Handler",
                                        $"Handling {packet.KnownPacketType} Voltron packet was not successful."));
                                    continue;
                                }
                                packetSendQueue.AddRange(returnPackets); // ADD TO SEND QUEUE
                            }
                            catch (Exception ex)
                            {
                                Telemetry.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Errors, "Voltron Handler",
                                        $"Handling the {packet.KnownPacketType} Voltron packet resulted in an error. \n" +
                                    $"{ex.Message}"));
                            }
                        }
                        bool disconnecting = false;
#if MAKEMANY
                        //MAKE ARIES FRAME WITH MANY VOlTRON PACKETS
                        var ariesPacket = TSOVoltronPacket.MakeVoltronAriesPacket(packetSendQueue);
                        ariesPacket.WritePacketToDisk(false);
                        Send(ID, ariesPacket);
#else
                        CloseAriesFrame(ID);
#endif
                    }
                    break;
            }
            Data.Dispose();
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
        private IEnumerable<TSOVoltronPacket> OnIncomingVoltronPacket(uint ID, TSOVoltronPacket Packet, ref bool Handled)
        {
            List<TSOVoltronPacket> packets = new();
            void defaultSend(TSOVoltronPacket packetToSend)
            {
                Debug_LogSendPDU(ID, packetToSend, NetworkTrafficDirections.CREATED);
                packets.Add(packetToSend);
            }

            Handled = true;
            switch (Packet.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU:
                    { // DB Request packets all start with the same Packet Type.
                        TSODBRequestWrapper? dbPacket = Packet as TSODBRequestWrapper;
                        if (dbPacket == default) break;
                        var clsID = (TSO_PreAlpha_DBActionCLSIDs)dbPacket.TSOSubMsgCLSID;
                        //Handle DB request packets here and enqueue the response packets into the return value of this function.
                        if (!_regulatorManager.HandleIncomingDBRequest(dbPacket, out TSOProtocolRegulatorResponse? returnValue) ||
                            returnValue == null)
                        { // handle failed!
                            Telemetry.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Discoveries, "TSODBRequestWrapper Handler",
                                        $"Could not handle " +
                                $"{(TSO_PreAlpha_DBStructCLSIDs)dbPacket.TSOPacketFormatCLSID}::" +
                                $"{(TSO_PreAlpha_DBActionCLSIDs)dbPacket.TSOSubMsgCLSID}!"));
                            break;
                        }

                        //handle success!
                        foreach (var packet in returnValue.ResponsePackets)
                            defaultSend(packet);
                    }
                    break;
                default: Handled = false; break;
            }
            // TRY USING THE REGULATOR MANAGER TO HANDLE THIS PACKET
            if (!Handled)
            { // INVOKE THE SERVER-SIDE REGULATOR MANAGER
                if (_regulatorManager.HandleIncomingPDU(Packet, out TSOProtocolRegulatorResponse Response))
                { // REGULATOR HANDLED
                    if (Response != null)
                    {
                        if (Response.InsertionPackets != null) // ADD INSERTIONS TO BACKLOG
                        {
                            foreach (var packet in Response.InsertionPackets)
                                _VoltronBacklog.Insert(0, packet);
                        }
                        if (Response.EnqueuePackets != null) // ADD INSERTIONS TO BACKLOG
                        {
                            foreach (var packet in Response.EnqueuePackets)
                                _VoltronBacklog.Add(packet);
                        }
                        if (Response.ResponsePackets != null) // ADD RESPONSE TO SENDQUEUE
                        {
                            foreach (var packet in Response.ResponsePackets)
                                defaultSend(packet);
                        }
                        Handled = true;
                    }
                }
            }
            return packets;
        }

        /// <summary>
        /// Takes the Outgoing Packets i
        /// </summary>
        private void CloseAriesFrame(uint ID)
        {
            bool disconnecting = false;
            foreach (var voltronPacket in packetSendQueue)
            {
                if (voltronPacket is TSOClientBye) disconnecting = true; // When sending the BYE_PDU we should disconnect from the Server cleanly.
                SubmitAriesFrame(ID, voltronPacket);
            }
            packetSendQueue.Clear();
            _VoltronBacklog.Clear();

            if (disconnecting)
                Disconnect(ID, SocketError.Disconnecting);
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
                Telemetry.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Message, Name,
                    $"***\n" +
                    $"\n" +
                    $"Split {voltronPacket.ToShortString()} into {largePDUs.Count()} {nameof(TSOSplitBufferPDU)}s... " +
                    $"({largePDUs.Count()} x {ClientBufferLength}(max) = {largePDUs.Sum(x => x.BodyLength)} bytes)\n" +
                    $"\n" +
                    $"***"));            
            foreach (var splitPDU in largePDUs)
            {
                TSOTCPPacket ariesPacket = TSOVoltronPacket.MakeVoltronAriesPacket(splitPDU);
                if (ariesPacket.BodyLength > ClientBufferLength)
                { // ERROR! Size too large!
                    throw new OverflowException("The size of the last TSOTCPPacket was way too large.");
                }
                Send(ID, ariesPacket);
                Telemetry.OnAriesPacket(NetworkTrafficDirections.OUTBOUND, DateTime.Now, ariesPacket, voltronPacket); // DEBUG LOGGING for ARIES packets
            }                        
            Telemetry.OnVoltronPacket(NetworkTrafficDirections.OUTBOUND, DateTime.Now, voltronPacket); // DEBUG LOGGING FOR VOLTRON PDUs
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
            uint splitSize = ClientBufferLength != 0 ? (uint)ClientBufferLength - TSOTCPPacket.ARIES_FRAME_HEADER_LEN : TSOSplitBufferPDU.STANDARD_CHUNK_SIZE;
            if (DBWrapper.BodyLength > splitSize && TestingConstraints.SplitBuffersPDUEnabled)
                packets.AddRange(TSOPDUFactory.CreateSplitBufferPacketsFromPDU(DBWrapper, splitSize));
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
            Telemetry.OnVoltronPacket(Direction, DateTime.Now, PDU, ID);

        /// <summary>
        /// Sends a <see cref="TSOVoltronPacket"/> in a new <see cref="TSOTCPPacket"/> from a DIFFERENT server.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="PDU"></param>
        public void SendPacket(ITSOServer Sender, TSOVoltronPacket PDU)
        {
            if (Sender != null)
                if (Sender == this) return;
            SubmitAriesFrame(_clientInfo.First().Key, PDU);
            if (Sender != null)
                Telemetry?.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Message,
                    nameof(TSOVoltronBasicServer),
                    $"Passed {PDU} from {Sender.GetType().Name} to {GetType().Name}"));
        }
    }
}

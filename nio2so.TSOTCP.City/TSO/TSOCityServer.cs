#define MAKEMANY
#undef MAKEMANY

using nio2so.Formats.BPS;
using nio2so.Formats.TSOData;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.City.TSO.Voltron.Regulator;
using QuazarAPI;
using QuazarAPI.Networking.Standard;
using System.Net;
using System.Net.Sockets;

namespace nio2so.TSOTCP.City.TSO
{
    /// <summary>
    /// Describes the flow of traffic for a network event
    /// </summary>
    public enum NetworkTrafficDirections
    {
        /// <summary>
        /// Default. None set.
        /// </summary>
        NONE,
        /// <summary>
        /// Traffic coming in from a remote connection
        /// </summary>
        INBOUND,
        /// <summary>
        /// Traffic going to a remote connection
        /// </summary>
        OUTBOUND,
        /// <summary>
        /// Traffic going to another recipient on the local machine
        /// </summary>
        LOCALMACHINE,
        /// <summary>
        /// A communication that has been created but not yet sent
        /// </summary>
        CREATED
    }

    /// <summary>
    /// The <see cref="TSOCityServer"/> is a The Sims Online Pre-Alpha Voltron Server analog.
    /// <para>It is designed to work with an unmodified copy of The Sims Online Pre-Alpha.</para>
    /// <para>It will by default host with a send/recv buffer allocated to be <see cref="TSO_Aries_SendRecvLimit"/></para>
    /// <para>To get the Client to connect to a <see cref="TSOCityServer"/>, ensure your TSOHTTPS server's ShardSelector points to the selected PORT,
    /// and you have modified your hosts file in System32/drivers/etc to include xo.max.ea.com</para>
    /// </summary>
    internal class TSOCityServer : QuazarServer<TSOTCPPacket>
    {
        public const UInt16 TSO_Aries_SendRecvLimit = 1024;

        public TSOCityTelemetryServer Telemetry { get; }

        public TSOCityServer(int port, IPAddress ListenIP = null) : base("TSOCity", port, 5, ListenIP)
        {
            SendAmount = ReceiveAmount = TSO_Aries_SendRecvLimit; // 1MB by default           
            CachePackets = false; // massive memory pit here if true
            DisposePacketOnSent = true; // no more memory leaks :)

            //**TELEMETRY
            Telemetry = new(this);
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

            var bps = BPSFileInterpreter.Interpret(@"E:\Games\TSO Pre-Alpha\packetlog.bps");

            //START THE SERVER
            BeginListening();
        }

        public override void Stop()
        {
            
        }

        protected override void OnClientConnect(TcpClient Connection, uint ID)
        {
            base.OnClientConnect(Connection, ID);

            //ARIES_GETCLIENTINFO
            Send(ID, new TSOTCPPacket(TSOAriesPacketTypes.ClientSessionInfo, 0, 0));            
        }

        protected override void OnIncomingPacket(uint ID, TSOTCPPacket Data)
        {
            //**Telemetry
            Telemetry.OnAriesPacket(NetworkTrafficDirections.INBOUND,DateTime.Now,Data);

            switch (Data.PacketType)
            {
                case (uint)TSOAriesPacketTypes.Client_SessionInfoResponse:
                    {                        
                        var sessionData = TSOAriesClientSessionInfo.FromPacket(Data);
                        QConsole.WriteLine($"Client: {ID}", sessionData.ToString());

                        //HOST_ONLINE_PDU
                        HandleSendClientHostOnlinePacket(ID);                        
                    }
                    break;
                case (uint)TSOAriesPacketTypes.Voltron:
                    {
                        bool tryHandler = true;
                        //GET VOLTRON PACKETS OUT OF DATA BUFFER
                        var packets = TSOVoltronPacket.ParseAllPackets(Data);
                        List<TSOVoltronPacket> packetSendQueue = new List<TSOVoltronPacket>(); // WE WILL SEND THESE TO CLIENT
                        foreach (var packet in packets)
                        {
                            //TSOVoltronPacket.MakeVoltronAriesPacket(packet).WritePacketToDisk(true,packet.KnownPacketType.ToString());
                            Debug_LogSendPDU(ID, packet, NetworkTrafficDirections.INBOUND); // DEBUG LOGGING
                            
                            if (!tryHandler) goto avoidHandlers;
                            try
                            {
                                bool _handled = false; // DICTATES WHETHER HANDLER WAS SUCCESSFUL
                                var returnPackets = OnIncomingVoltronPacket(ID, packet, ref _handled);
                                if (!_handled)
                                { // HANDLER FAILED
                                    Telemetry.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Warnings,"Voltron Handler",
                                        $"Handling {packet.KnownPacketType} Voltron packet was not successful."));
                                    continue;
                                }
                                packetSendQueue.AddRange(returnPackets); // ADD TO SEND QUEUE
                            }
                            catch (Exception ex)
                            {
                                Telemetry.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Errors, "Voltron Handler",
                                        $"Handling the {packet.KnownPacketType} Voltron packet resulted in an error. \n" +
                                    $"{ex.Message}"));
                            }
                        }
#if MAKEMANY
                        //MAKE ARIES FRAME WITH MANY VOlTRON PACKETS
                        var ariesPacket = TSOVoltronPacket.MakeVoltronAriesPacket(packetSendQueue);
                        ariesPacket.WritePacketToDisk(false);
                        Send(ID, ariesPacket);
#else
                        foreach (var voltronPacket in packetSendQueue)
                        {
                            var ariesPacket = TSOVoltronPacket.MakeVoltronAriesPacket(voltronPacket);
                            Send(ID, ariesPacket);
                            Telemetry.OnAriesPacket(NetworkTrafficDirections.OUTBOUND, DateTime.Now, ariesPacket, voltronPacket); // DEBUG LOGGING
                        }
#endif
                    avoidHandlers:
                        ;
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
                case TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU:
                    //SEND AN UPDATE_PLAYER_PDU
                    defaultSend(HandleSendClientPlayerOnlinePacket());
                    Handled = true;
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU:
                    {
                        /* From niotso:                  hello, fatbag - bisquick :]
                         * TODO: It is known that sending HouseSimConstraintsResponsePDU (before the
	                    ** GetCharBlobByID response and other packets) is necessary for the game to post
	                    ** kMSGID_LoadHouse and progress HouseLoadRegulator from kStartedState to kLoadingState.
	                    ** However, the game appears to never send HouseSimConstraintsPDU to the server at
	                    ** any point.
	                    **
	                    ** The question is:
	                    ** Is there a packet that we can send to the game to have it send us
	                    ** HouseSimConstraintsPDU?
	                    ** Actually, (in New & Improved at least), manually sending a kMSGID_LoadHouse packet
	                    ** to the client (in an RSGZWrapperPDU) will cause the client to send
	                    ** HouseSimConstraintsPDU to the server.
	                    ** It is not known at this point if that is the "correct" thing to do.
	                    **
	                    ** So, for now, we will send the response packet to the game, without it having explicitly
	                    ** sent us a request packet--just like (in response to HostOnlinePDU) the game sends us a
	                    ** LoadHouseResponsePDU without us ever having sent it a LoadHousePDU. ;)
	                    */
                        var houseID = ((TSOLoadHouseResponsePDU)Packet).HouseID;
                        defaultSend(new TSOHouseSimConstraintsResponsePDU(TSOVoltronConst.MyHouseID)); // dictate what lot to load here.
                        Handled = true;
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU:
                    {
                        TSOBCVersionListPDU pdu = (TSOBCVersionListPDU)Packet;
                        defaultSend(new TSOBCVersionListPDU(pdu.VersionString, "", 0x01));
                        Handled = false;
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU:
                    { // DB Request packets all start with the same Packet Type.
                        TSODBRequestWrapper? dbPacket = Packet as TSODBRequestWrapper;
                        if (dbPacket == default) break;
                        var clsID = (TSO_PreAlpha_DBActionCLSIDs)dbPacket.TSOSubMsgCLSID;
                        //Handle DB request packets here and enqueue the response packets into the return value of this function.
                        if (!TSORegulatorManager.HandleIncomingDBRequest(dbPacket, out var returnpackets))
                        { // handle failed!
                            Telemetry.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Discoveries,  "TSODBRequestWrapper Handler",
                                        $"Could not handle " +
                                $"{(TSO_PreAlpha_DBStructCLSIDs)dbPacket.TSOPacketFormatCLSID}::" +
                                $"{(TSO_PreAlpha_DBActionCLSIDs)dbPacket.TSOSubMsgCLSID}!"));
                            break;
                        }
                        Telemetry.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message,  "TSODBRequestWrapper Handler",
                                        $"Handled " +
                                $"{(TSO_PreAlpha_DBStructCLSIDs)dbPacket.TSOPacketFormatCLSID}::" +
                                $"{(TSO_PreAlpha_DBActionCLSIDs)dbPacket.TSOSubMsgCLSID}!"));
                        //handle success!
                        foreach (var packet in returnpackets)    
                            defaultSend(packet);
                    }
                    break;
                default: Handled = false; break;
            }
            // TRY USING THE REGULATOR MANAGER TO HANDLE THIS PACKET
            if (!Handled)
            { // INVOKE THE SERVER-SIDE REGULATOR MANAGER
                if (TSORegulatorManager.HandleIncomingPDU(Packet, out var returnPDUs))
                {
                    foreach (var packet in returnPDUs)
                        defaultSend(Packet);
                    Handled = true;                    
                }
            }
            return packets;
        }

        protected override void OnOutgoingPacket(uint ID, TSOTCPPacket Data)
        {
            
        }

        #region HANDLERS
        /// <summary>
        /// Handles an incoming connection by sending it the <see cref="TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PacketProperties"></param>
        private void HandleSendClientHostOnlinePacket(uint ID, TSOHostOnlinePDU? PacketProperties = default)
        {
            if (PacketProperties == default) PacketProperties = new TSOHostOnlinePDU();
            var sendPacket = TSOVoltronPacket.MakeVoltronAriesPacket(PacketProperties); //new TSOHostOnlinePDU(1, 1024, "Hello", "World"));            
            Send(ID, sendPacket);      
            Debug_LogSendPDU(ID, PacketProperties, NetworkTrafficDirections.CREATED);      
        }
        /// <summary>
        /// Handles an incoming connection by sending it the <see cref="TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PacketProperties"></param>
        private TSOUpdatePlayerPDU HandleSendClientPlayerOnlinePacket(TSOUpdatePlayerPDU? PacketProperties = default)
        {
            if (PacketProperties == default) 
                PacketProperties = new TSOUpdatePlayerPDU("161", "Bloaty");
            return PacketProperties;
        }
        #endregion
        /// <summary>
        /// Logs to the debug output stream the packet's information so it can be assessed when debugging.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PDU"></param>
        /// <param name="Verb"></param>
        private void Debug_LogSendPDU(uint ID, TSOVoltronPacket PDU, NetworkTrafficDirections Direction) => 
            Telemetry.OnVoltronPacket(Direction,DateTime.Now,PDU,ID);
    }
}

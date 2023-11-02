using nio2so.TSOTCP.City.TSO.Aries;
using QuazarAPI.Networking.Data;
using QuazarAPI;
using QuazarAPI.Networking.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using nio2so.Formats.TSOData;

namespace nio2so.TSOTCP.City.TSO
{
    internal class TSOCityServer : QuazarServer<TSOTCPPacket>
    {
        public const UInt16 TSO_Aries_SendRecvLimit = 1024;

        public TSOCityServer(int port, IPAddress ListenIP = null) : base("TSOCity", port, 5, ListenIP)
        {
            ReceiveAmount = TSO_Aries_SendRecvLimit;
            CachePackets = false;
            DisposePacketOnSent = true;
        }

        public override void Start()
        {
            var file = TSODataImporter.Import(@"E:\Games\TSO Pre-Alpha\TSO\TSOData_DataDefinition.dat");
            File.WriteAllText("/packets/datadefinition.json", file.ToString());

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
            Data.WritePacketToDisk();
            switch (Data.PacketType)
            {
                case (uint)TSOAriesPacketTypes.Client_SessionInfoResponse:
                    {
                        var sessionData = TSOAriesClientSessionInfo.FromPacket(Data);
                        QConsole.WriteLine($"Client: {ID}", sessionData.ToString());

                        //HOST_ONLINE_PDU
                        HandleSendClientHostOnlinePacket(ID);
                        //UPDATE_PLAYER_PDU
                        HandleSendClientPlayerOnlinePacket(ID);
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
                            Debug_LogSendPDU(ID, packet, "Received"); // DEBUG LOGGING
                            if (!tryHandler) goto avoidHandlers;
                            try
                            {
                                bool _handled = false; // DICTATES WHETHER HANDLER WAS SUCCESSFUL
                                var returnPackets = OnIncomingVoltronPacket(ID, packet, ref _handled);
                                if (!_handled)
                                { // HANDLER FAILED
                                    QConsole.WriteLine("TSO City Server Warnings", "Handling the aforementioned Voltron packet was not successful.");
                                    continue;
                                }
                                packetSendQueue.AddRange(returnPackets); // ADD TO SEND QUEUE
                            }
                            catch (Exception ex)
                            {
                                QConsole.WriteLine("TSO City Server Warnings", $"Handling the aforementioned Voltron packet resulted in an error. {ex.Message}");
                            }
                        }
                        //MAKE ARIES FRAME WITH MANY VOlTRON PACKETS
                        var ariesPacket = TSOVoltronPacket.MakeVoltronAriesPacket(packetSendQueue);
                        ariesPacket.WritePacketToDisk(false);
                        Send(ID, ariesPacket);
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
                Debug_LogSendPDU(ID, packetToSend);
                packets.Add(packetToSend);               
            }

            if (Handled) return packets;
            //Handled = true;
            switch (Packet.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU:
                    {
                        var formattedPacket = (TSOSetAcceptAlertsPDU)Packet;
                        defaultSend(new TSOSetAcceptAlertsResponsePDU(true));
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU:                    
                    defaultSend(new TSOSetAcceptFlashesResponsePDU(true));
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU:                    
                    defaultSend(new TSOSetIgnoreListResponsePDU());
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU:
                    defaultSend(new TSOSetInvincibleResponsePDU(true));
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU:
                    defaultSend(new TSOSetInvisibleResponsePDU(true));
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
                        defaultSend(new TSOHouseSimConstraintsResponsePDU());
                        Handled = true;
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU:
                    { // DB Request packets all start with the same Packet Type.
                        TSODBRequestWrapper? dbPacket = Packet as TSODBRequestWrapper;
                        if (dbPacket == default) break;
                        //Handle DB request packets here and enqueue the response packets into the return value of this function.
                        TSODBRequestWrapper? returnDBPacket = HandleDBWrapperPDUPacket(dbPacket);
                        if (returnDBPacket == default)
                        { // handle failed!
                            QConsole.WriteLine("TSODBRequestWrapper Handler", $"Could not handle " +
                                $"{(TSO_PreAlpha_DBStructCLSIDs)dbPacket.TSOPacketFormatCLSID}::" +
                                $"{(TSO_PreAlpha_DBActionCLSIDs)dbPacket.TSOSubMsgCLSID}!");
                            break;
                        }
                        //handle success!
                        defaultSend(returnDBPacket);
                        Handled = true;
                    }
                    break;
                default: Handled = false; break;
            }
            return packets;
        }

        protected override void OnOutgoingPacket(uint ID, TSOTCPPacket Data)
        {
            
        }

        #region HANDLERS
        private TSODBRequestWrapper? HandleDBWrapperPDUPacket(TSODBRequestWrapper DBPacket)
        {
            switch ((TSO_PreAlpha_DBStructCLSIDs)DBPacket.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard:
                    { // TSO Net Message Standard type responses (most common)
                        switch ((TSO_PreAlpha_DBActionCLSIDs)DBPacket.TSOSubMsgCLSID)
                        {
                            case TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotIDRequest:
                                return new TSOGetRoommateInfoByLotIDResponse(DBPacket.AriesID, DBPacket.MasterID);
                        }
                    }
                    break;
            }
            return null;
        }
        /// <summary>
        /// Handles an incoming connection by sending it the <see cref="TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PacketProperties"></param>
        private void HandleSendClientHostOnlinePacket(uint ID, TSOHostOnlinePDU? PacketProperties = default)
        {
            if (PacketProperties == default) PacketProperties = new TSOHostOnlinePDU();
            var sendPacket = TSOVoltronPacket.MakeVoltronAriesPacket(PacketProperties); //new TSOHostOnlinePDU(1, 1024, "Hello", "World"));
            sendPacket.WritePacketToDisk(false);
            Send(ID, sendPacket);      
            Debug_LogSendPDU(ID, PacketProperties);      
        }
        /// <summary>
        /// Handles an incoming connection by sending it the <see cref="TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PacketProperties"></param>
        private void HandleSendClientPlayerOnlinePacket(uint ID, TSOUpdatePlayerPDU? PacketProperties = default)
        {
            if (PacketProperties == default) PacketProperties = new TSOUpdatePlayerPDU("1337", "asdf");
            var sendPacket = TSOVoltronPacket.MakeVoltronAriesPacket(PacketProperties); //new TSOHostOnlinePDU(1, 1024, "Hello", "World"));
            sendPacket.WritePacketToDisk(false);
            Send(ID, sendPacket);
            Debug_LogSendPDU(ID, PacketProperties);
        }
        #endregion
        /// <summary>
        /// Logs to the debug output stream the packet's information so it can be assessed when debugging.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PDU"></param>
        /// <param name="Verb"></param>
        private void Debug_LogSendPDU(uint ID, TSOVoltronPacket PDU, string Verb = "Sent") => QConsole.WriteLine($"Client: {ID}", $"{Verb} the {PDU}");
    }
}

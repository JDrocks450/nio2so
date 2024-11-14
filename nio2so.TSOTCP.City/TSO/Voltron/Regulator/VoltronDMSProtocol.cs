using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    [Voltron.Regulator.TSORegulator("VoltronDMSProtocol")]
    /// <summary>
    /// This protocol handles lower-level functions of the Voltron Data Service such as 
    /// <see cref="TSOHostOnlinePDU"/>, <see cref="TSOClientByePDU"/>
    /// </summary>
    internal class VoltronDMSProtocol : ITSOProtocolRegulator
    {
        public string RegulatorName => "VoltronDMSProtocol";

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            Response = default;
            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> returnPackets = new List<TSOVoltronPacket>();
            Response = new(returnPackets, null, null);

            bool success = false;
            void defaultSend(TSOVoltronPacket outgoing)
            {
                returnPackets.Add(outgoing);
                success = true;
            }

            // Disable these for now
            switch (PDU.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.BYE_PDU:                    
                    var bye_pdu = (TSOClientBye)PDU;
                    LogMessage("Client is saying Bye! Disconnecting after frame...");
                    defaultSend(new TSOClientBye(bye_pdu.StatusCode, bye_pdu.Message));
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU:
                    //SEND AN UPDATE_PLAYER_PDU
                    defaultSend(HandleSendClientPlayerOnlinePacket());
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

                        success = true;
                        var houseID = ((TSOLoadHouseResponsePDU)PDU).HouseID;

                        //The client will always send a LoadHouseResponsePDU with a blank houseID, so this can be ignored
                        //when that happens
                        if (houseID == 0)
                            break;

                        defaultSend(new TSOHouseSimConstraintsResponsePDU(TSOVoltronConst.MyHouseID)); // dictate what lot to load here.                        
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.BC_VERSION_LIST_PDU:
                    {
                        TSOBCVersionListPDU pdu = (TSOBCVersionListPDU)PDU;
                        defaultSend(new TSOBCVersionListPDU(pdu.VersionString, "", 0x01));
                        success = false;
                    }
                    break;
            }
            return success;
        }

        private void LogMessage(string Message)
        {
            TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message,
                RegulatorName, Message));
        }

        /// <summary>
        /// Handles an incoming connection by sending it the <see cref="TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="PacketProperties"></param>
        private TSOUpdatePlayerPDU HandleSendClientPlayerOnlinePacket(TSOUpdatePlayerPDU? PacketProperties = default)
        {
            if (PacketProperties == default)
                PacketProperties = new TSOUpdatePlayerPDU(TSOVoltronConst.MyAvatarID.ToString(), TSOVoltronConst.MyAvatarName);
            return PacketProperties;
        }
    }
}

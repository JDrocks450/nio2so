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

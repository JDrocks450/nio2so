using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{   
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator("AvatarProtocol")]
    internal class AvatarProtocol : ITSOProtocolRegulator
    {
        public string RegulatorName => "AvatarProtocol";

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> returnPackets = new();
            Response = new(returnPackets, null, null);

            switch ((TSO_PreAlpha_DBStructCLSIDs)PDU.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard:
                    { // TSO Net Message Standard type responses (most common)
                        var clsID = (TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID;
                        switch (clsID)
                        {
                            case TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByIDRequest:
                                returnPackets.Add(new TSOGetCharBlobByIDResponse(PDU.AriesID, PDU.MasterID, PDU.TransactionID, TSOVoltronConst.MyAvatarID));
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetBookmarksRequest:
                                returnPackets.Add(new TSOGetBookmarksResponse(PDU.AriesID, PDU.MasterID, PDU.TransactionID));
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request:
                                ;
                                return false;
                            case TSO_PreAlpha_DBActionCLSIDs.InsertGenericLog_Request:
                                string message = PDU.MessageString;
                                TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message, 
                                    "cDBServiceClientD", "[InsertGenericLog_Request] " + message));
                                return true;
                        }
                    }
                    break;
            }

            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> returnPackets = new List<TSOVoltronPacket>();
            Response = new(returnPackets,null,null);

            bool success = false;
            void defaultSend(TSOVoltronPacket outgoing)
            {
                returnPackets.Add(outgoing);
                success = true;
            }

            return success; 
            
            // Disable these for now
            switch (PDU.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU:
                    {
                        var formattedPacket = (TSOSetAcceptAlertsPDU)PDU;
                        defaultSend(new TSOSetAcceptAlertsResponsePDU(true));
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU:
                    defaultSend(new TSOSetAcceptFlashesResponsePDU(true));
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_IGNORE_LIST_PDU:
                    defaultSend(new TSOSetIgnoreListResponsePDU(true));
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVISIBLE_PDU:
                    defaultSend(new TSOSetInvincibleResponsePDU(true));
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.SET_INVINCIBLE_PDU:
                    defaultSend(new TSOSetInvisibleResponsePDU(true));
                    break;
            }
            return success;
        }
    }
}

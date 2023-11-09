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
    [TSORegulator("LotProtocol")]
    internal class LotProtocol : ITSOProtocolRegulator
    {
        public string RegulatorName => "LotProtocol";

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out IEnumerable<TSOVoltronPacket> ResponsePackets)
        {
            List<TSOVoltronPacket> returnPackets = new();
            ResponsePackets = returnPackets;

            switch ((TSO_PreAlpha_DBStructCLSIDs)PDU.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard:
                    { // TSO Net Message Standard type responses (most common)
                        switch ((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID)
                        {
                            case TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotIDRequest: 
                                returnPackets.Add(new TSOGetRoommateInfoByLotIDResponse(PDU.AriesID, PDU.MasterID, PDU.TransactionID));
                                return true;
                        }
                    }
                    break;
            }

            ResponsePackets = null;
            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out IEnumerable<TSOVoltronPacket> ResponsePackets)
        {
            List<TSOVoltronPacket> returnPackets = new List<TSOVoltronPacket>();
            ResponsePackets = returnPackets;

            bool success = false;
            void defaultSend(TSOVoltronPacket outgoing)
            {
                returnPackets.Add(outgoing);
                success = true;
            }

            switch (PDU.KnownPacketType)
            {
                default: break;
            }
            return success;
        }
    }
}

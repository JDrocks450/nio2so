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

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> returnPackets = new();
            Response = new(returnPackets, null, null);

            switch ((TSO_PreAlpha_DBStructCLSIDs)PDU.TSOPacketFormatCLSID)
            {
                case TSO_PreAlpha_DBStructCLSIDs.GZCLSID_cCrDMStandardMessage:
                    { // TSO Net Message Standard type responses (most common)
                        switch ((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID)
                        {
                            case TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotIDRequest:
                                {
                                    if (!PDU.HasData1)
                                        return false;
                                    uint HouseID = PDU.Data1.Value; // DATA1 is HouseID
                                    returnPackets.Add(new TSOGetRoommateInfoByLotIDResponse(PDU.AriesID, PDU.MasterID,HouseID,161));                                        
                                }
                                return true;
                            case TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByIDRequest:
                                {
                                    uint HouseID = PDU.Data1.Value; // DATA1 is HouseID
                                    returnPackets.Add(new TSOGetHouseBlobByIDResponse(PDU.AriesID, PDU.MasterID, PDU.kMSGID, HouseID));                                        
                                }
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

            switch (PDU.KnownPacketType)
            {
                default: break;
            }
            return success;
        }
    }
}

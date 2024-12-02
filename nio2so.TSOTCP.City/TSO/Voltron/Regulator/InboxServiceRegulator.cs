using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    /// <summary>
    /// Handles all requests pertaining to the Message Inbox in The Sims Online: Pre-Alpha
    /// </summary>
    [TSORegulator(nameof(InboxServiceRegulator))]
    internal class InboxServiceRegulator : ITSOProtocolRegulator
    {
        public string RegulatorName => nameof(InboxServiceRegulator);

        public bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            //cannot handle DB requests there are none for the inbox service in TSO.
            Response = null;
            return false;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            List<TSOVoltronPacket> responsePackets = new();
            Response = new(responsePackets, null, null);

            switch (PDU.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_PDU:
                    { // get all of my messages
                        responsePackets.Add(new TSOGetMPSMessagesPDUResponse());
                    }
                    return true;
            }

            Response = null;
            return false; // cannot
        }
    }
}

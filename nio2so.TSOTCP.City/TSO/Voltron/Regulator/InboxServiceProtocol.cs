﻿using nio2so.TSOTCP.City.TSO.Voltron.PDU;
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
    [TSORegulator(nameof(InboxServiceProtocol))]
    internal class InboxServiceProtocol : TSOProtocol
    {
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_PDU)]
        public void GET_MPS_MESSAGES_PDU(TSOVoltronPacket PDU)
        { // get all of my messages
            RespondWith(new TSOGetMPSMessagesPDUResponse());
        }
    }
}

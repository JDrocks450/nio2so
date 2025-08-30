using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator("Top 100 Protocol")]
    internal class Top100Protocol : TSOProtocol
    {
        [TSOProtocolDatabaseHandler((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopList_Request)]
        public void GET_TOP_LIST_REQUEST(TSODBRequestWrapper DBPDU)
        {
            //trigger exp to find spot to breakpoint in the disassembly
            //RespondTo(DBPDU, new TSODebugWrapperPDU(new byte[0], TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response));
            //return;
            //respond with test pdu
            RespondTo(DBPDU, TSODebugWrapperPDU.FromFile(@"C:\nio2so\const\gettop100list.dat", TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response));
        }
    }
}

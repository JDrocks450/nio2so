using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopList_Request)]
    public class TSOGetTopListRequest : TSODBRequestWrapper
    {

    }    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetBookmarks_Request)]
    internal class TSOGetBookmarksRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Needs further testing to confirm
        /// </summary>
        [TSOVoltronDBWrapperField] public uint ListType { get; set; }

        public TSOGetBookmarksRequest() : base()
        {

        }
    }
}

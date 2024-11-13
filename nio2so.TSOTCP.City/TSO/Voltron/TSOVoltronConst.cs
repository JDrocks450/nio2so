using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    internal static class TSOVoltronConst
    {
        public const uint ResponsePDU_DefaultStatusCode = 0;
        public const string ResponsePDU_DefaultReasonText = "OK.";

        //****BETA TESTING
        public const uint MyHouseID = 0x053A; //0x053A; // 1338 // can be zero for testing as well
        public const uint MyAvatarID = 0xA1; // 161
        //****
    }
}

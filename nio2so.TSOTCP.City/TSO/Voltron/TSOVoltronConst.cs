using nio2so.Data.Common.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    /// <summary>
    /// Constants to use for the <see cref="TSOCityServer"/>
    /// </summary>
    internal static class TSOVoltronConst
    {
        public const uint ResponsePDU_DefaultStatusCode = 0;
        public const string ResponsePDU_DefaultReasonText = "OK.";
        public const byte SplitBufferPDU_DefaultChunkSize = 0xB4;

        //****WORKSPACE
        public const string WorkspaceDirectory = TestingConstraints.WorkspaceDirectory;
        public const string SysLogPath = WorkspaceDirectory + "/nio2so_syslog.txt";
        public const string DiscoveriesDirectory = WorkspaceDirectory + "/discoveries";
        public const string AriesPacketDirectory = WorkspaceDirectory + "/tsotcppackets";
        public const string VoltronPacketDirectory = WorkspaceDirectory + "/tsotcppackets";
        public const string HouseDataDirectory = WorkspaceDirectory + "/house";
        public const string AvatarDataDirectory = WorkspaceDirectory + "/avatar";
        //****

        //****BETA TESTING
        public const uint MyHouseID = TestingConstraints.MyHouseID; //0x053A; // 1338 // can be zero for testing as well
        public const uint MyAvatarID = TestingConstraints.MyAvatarID; // 161
        public const string MyAvatarName = TestingConstraints.MyAvatarName;        
        //****
    }
}

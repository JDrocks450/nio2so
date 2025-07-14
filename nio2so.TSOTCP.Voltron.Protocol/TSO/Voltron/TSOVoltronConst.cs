using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Aries;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron
{
    /// <summary>
    /// Constants to use for the <see cref="TSOCityServer"/>
    /// </summary>
    public static class TSOVoltronConst
    {
        public const uint ResponsePDU_DefaultStatusCode = 0;
        public const string ResponsePDU_DefaultReasonText = "OK.";

        public const ushort TSOAriesClientBufferLength = 0x200;
        public const uint SplitBufferPDU_DefaultChunkSize = TSOAriesClientBufferLength - TSOTCPPacket.ARIES_FRAME_HEADER_LEN;

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
        public const uint MyAvatarID = TestingConstraints.MyAvatarID; // 0x0539
        public const string MyAvatarName = TestingConstraints.MyAvatarName;
        //****
    }
}

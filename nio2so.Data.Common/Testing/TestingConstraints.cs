namespace nio2so.Data.Common.Testing
{
    /// <summary>
    /// Constraints for Beta Testing
    /// </summary>
    public static class TestingConstraints
    {
        //****BETA TESTING

        public const bool LogPackets = true;
        public const bool LogAriesPackets = false;
        public const bool LogVoltronPackets = true;

        public const int City_ListenPort = 49100; // 49101 HouseSimServer test and 49100 for City Server
        public const int Room_ListenPort = 49101; // 49101 HouseSimServer test and 49100 for City Server

        /// <summary>
        /// Dictates whether the api will automagically split large PDUs to smaller ones.
        /// </summary>
        public static bool SplitBuffersPDUEnabled = true;
        /// <summary>
        /// Transport layer and ServerTickConfirmation messages are logged
        /// </summary>
        public const bool VerboseLogging = false;

        /// <summary>
        /// What the ID for My House
        /// </summary>
        public const string MyHouseName = "First House";
        public const uint MyHouseLotID = 6094983;
        /// <summary>
        /// What the ID for my Avatar is
        /// </summary>
        public const uint MyAvatarID = 0x0539; // 1337 //0xA1; // 161
        /// <summary>
        /// What my avatar's name is
        /// </summary>
        public const string MyAvatarName = "bisquick"; // "Bisquick" //"JollySim";
        /// <summary>
        /// What shard name to use as the Default Shard
        /// </summary>
        public const string MyShardName = "Blazing Falls";
        /// <summary>
        /// Set the first Sim to always be blank after logging in
        /// </summary>
        public const bool CASTestingMode = false;        
        /// <summary>
        /// Set the UserAuth servlet to mark you as a CSR
        /// </summary>
        public const bool CSRUserAuth = false;

        /// <summary>
        /// For testing, this is the AvatarID of "MyFriend" as in-your first bookmark, your friend in the relationship web, etc.
        /// </summary>
        public const uint MyFriendAvatarID = 161;

        /// <summary>
        /// The static amount of funds in your avatar's account
        /// </summary>
        public const int StartingFunds = 500000;
        //****

        //**PATHS
        public const string WorkspaceDirectory = @"C:\nio2so";
        public const string DatabaseDirectory = WorkspaceDirectory + @"\db";

        public const string MyFriendAvatarName = "FriendlyBuddy";

        //**TEST
        /// <summary>
        /// Will set the server to always redirect a client into the HSB mode (hosting a lot without an avatar there)
        /// <para/>Should be true if using the HSB test
        /// </summary>
        public const uint HSBAutoJoinHouseID = MyHouseLotID;
    }
}

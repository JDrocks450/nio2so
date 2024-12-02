using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Data.Common.Testing
{
    public static class TestingConstraints
    {
        //****BETA TESTING

        public const int ListenPort = 49100; // 49101 HouseSimServer test and 49100 for City Server

        //**Default Account
        public const string LoginUsername = "bloaty";
        public const string LoginPassword = "asdf";
        //**

        /// <summary>
        /// What the ID for My House
        /// </summary>
        public const uint MyHouseID = 0x053A; // 1338 // can be zero for testing as well
        /// <summary>
        /// What the ID for my Avatar is
        /// </summary>
        public const uint MyAvatarID = 0x0539; // 1337 //0xA1; // 161
        /// <summary>
        /// What my avatar's name is
        /// </summary>
        public const string MyAvatarName = "bisuqick"; // "Bisquick" //"JollySim";
        /// <summary>
        /// What shard name to use as the Default Shard
        /// </summary>
        public const string MyShardName = "Blazing Falls";
        /// <summary>
        /// Set the first Sim to always be blank after logging in
        /// </summary>
        public const bool CASTestingMode = false;
        /// <summary>
        /// Will set the server to always redirect a client into a seemingly offline testing lot
        /// </summary>
        public const bool LOTTestingMode = true;
        /// <summary>
        /// When entering CAS the Shard-Selector will generate an AvatarID -- you can set it here
        /// </summary>
        public const uint CASAvatarID = 161;
        /// <summary>
        /// Set the UserAuth servlet to mark you as a CSR
        /// </summary>
        public const bool CSRUserAuth = false;

        /// <summary>
        /// For testing, this is the AvatarID of "MyFriend" as in-your first bookmark, your friend in the relationship web, etc.
        /// </summary>
        public const uint MyFriendAvatarID = 161;
        //****

        //**PATHS
        public const string WorkspaceDirectory = "/packets";
        public const string AvatarDataDictionaryFileName = WorkspaceDirectory + "/db/avatardataservlet.json";
        public const string UserAccountDictionaryFileName = WorkspaceDirectory + "/db/useraccounts.json";

        //**VERY TEMPORARY
        /// <summary>
        /// Sends a GetHouseBlobByID_Response blob that, while wrongly formatted, strangely works to at least get onto a lot, right now.
        /// </summary>
        public const bool JustGetMeToLotView = false;
        public const string MyFriendAvatarName = "FriendlyBuddy";
    }
}

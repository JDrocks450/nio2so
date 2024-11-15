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
        public const string LoginUsername = "bloaty";
        public const string LoginPassword = "asdf";

        /// <summary>
        /// What the ID for My House
        /// </summary>
        public const uint MyHouseID = 0x053A; //0x053A; // 1338 // can be zero for testing as well
        /// <summary>
        /// What the ID for my Avatar is
        /// </summary>
        public const uint MyAvatarID = 0x0539; //0xA1; // 161
        /// <summary>
        /// What my avatar's name is
        /// </summary>
        public const string MyAvatarName = "JollySim"; //"bisuqick";
        /// <summary>
        /// What shard name to use as the Default Shard
        /// </summary>
        public const string MyShardName = "Blazing Falls";
        /// <summary>
        /// Set the first Sim to always be blank after logging in
        /// </summary>
        public const bool CASTestingMode = false;
        /// <summary>
        /// When entering CAS the Shard-Selector will generate an AvatarID -- you can set it here
        /// </summary>
        public const uint CASAvatarID = 0x0;
        /// <summary>
        /// Set the UserAuth servlet to mark you as a CSR
        /// </summary>
        public const bool CSRUserAuth = false;
        //****
    }
}

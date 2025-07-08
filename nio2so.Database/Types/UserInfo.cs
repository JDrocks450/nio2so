using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Database.Types
{
    public class UserInfo
    {
        public UserInfo()
        {

        }

        public UserInfo(uint userId, string userName, HashSet<uint> avatars)
        {
            UserId = userId;
            UserName = userName;
            Avatars = avatars;
        }

        /// <summary>
        /// The ID of this User's profile        
        /// </summary>
        public uint UserId { get; set; }
        /// <summary>
        /// This User's UserName
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The IDs of avatars that belong to this User        
        /// </summary>
        public HashSet<uint> Avatars { get; set; }

        /// <summary>
        /// A <see cref="UserInfo"/> default value for the creator of this project, me :)
        /// </summary>
        public static UserInfo Bloaty => new(1, "Bloaty", new() { 1337 });
    }
}

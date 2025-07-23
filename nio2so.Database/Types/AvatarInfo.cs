using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Types
{
    public class AvatarInfo
    {
        public AvatarInfo() { }
        public AvatarInfo(UserToken accountOwner, AvatarIDToken AvatarID) : this()
        {
            AccountOwner = accountOwner;
            this.AvatarID = AvatarID;            
        }

        /// <summary>
        /// Stores information on Bookmarks for an individual avatar
        /// </summary>
        public class AvatarBookmarkInfo
        {
            public HashSet<AvatarIDToken> BookmarkAvatars { get; set; } = new();
        }

        /// <summary>
        /// The account owner of this avatar
        /// </summary>
        public UserToken AccountOwner { get; set; }
        /// <summary>
        /// The ID of this Avatar
        /// </summary>
        public AvatarIDToken AvatarID { get; set; }
        /// <summary>
        /// The amount of friends this avatar has
        /// </summary>
        public byte Friends { get; set; } = 0;
        /// <summary>
        /// Which Shard this Avatar lives on
        /// </summary>
        public string Shard { get; set; } = TestingConstraints.MyShardName;
        /// <summary>
        /// What service this avatar was created with
        /// </summary>
        public string CreatedUsing { get; set; }

        /// <summary>
        /// The profile for this avatar; containing basic information about it
        /// </summary>
        public AvatarProfile Profile => new(AvatarID, AvatarCharacter.AvatarName, AvatarCharacter.Funds, 0, Friends, 0, Shard);

        /// <summary>
        /// A store of bookmarks added for this avatar
        /// </summary>
        public AvatarBookmarkInfo BookmarkInfo { get; set; } = new();

        /// <summary>
        /// The <see cref="TSODBChar"/> file attached to this avatar
        /// </summary>
        public TSODBChar AvatarCharacter { get; set; } = new();
    }
}

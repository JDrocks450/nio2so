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
        /// <summary>
        /// The owner of this avatar
        /// </summary>
        public UserToken AccountOwner { get; set; }

        /// <summary>
        /// The profile for this avatar; containing basic information about it
        /// </summary>
        public AvatarProfile Profile { get; set; }
    }
}

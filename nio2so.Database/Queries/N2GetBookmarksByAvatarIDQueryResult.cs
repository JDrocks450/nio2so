using nio2so.DataService.Common.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Queries
{
    public record N2BookmarksByAvatarIDQueryResult(AvatarIDToken AvatarID, IEnumerable<AvatarIDToken> Avatars);
}

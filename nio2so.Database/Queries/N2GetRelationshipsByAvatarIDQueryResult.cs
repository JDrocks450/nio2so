using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Queries
{
    public record N2RelationshipsByAvatarIDQueryResult(AvatarIDToken TargetID, string Direction, IEnumerable<AvatarRelationship> Relationships);
}

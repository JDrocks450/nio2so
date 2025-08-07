using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;

namespace nio2so.DataService.Common.Queries
{
    public record N2RelationshipsByAvatarIDQueryResult(AvatarIDToken TargetID, string Direction, IEnumerable<AvatarRelationship> Relationships);
}

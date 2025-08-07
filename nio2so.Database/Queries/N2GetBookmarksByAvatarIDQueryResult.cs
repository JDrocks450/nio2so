using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.Common.Queries
{
    public record N2BookmarksByAvatarIDQueryResult(AvatarIDToken AvatarID, IEnumerable<AvatarIDToken> Avatars);
}

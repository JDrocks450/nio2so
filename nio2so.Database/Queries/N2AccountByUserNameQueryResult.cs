using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.Common.Queries
{
    /// <summary>
    /// Structure for the result of the call to account resource by UserName
    /// </summary>
    /// <param name="UserNameQuery">The query string</param>
    /// <param name="ServerUserToken">The return <see cref="UserToken"/> from the Server</param>
    public record N2AccountByUserNameQueryResult(string UserNameQuery, UserToken ServerUserToken);
}

using nio2so.DataService.Common.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Queries
{
    /// <summary>
    /// Structure for the result of the call to account resource by UserName
    /// </summary>
    /// <param name="UserNameQuery">The query string</param>
    /// <param name="ServerUserToken">The return <see cref="UserToken"/> from the Server</param>
    public record N2AccountByUserNameQueryResult(string UserNameQuery, UserToken ServerUserToken);
}

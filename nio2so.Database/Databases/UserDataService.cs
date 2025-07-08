using nio2so.Database.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Database.Databases
{
    public class UserDataService : DatabaseComponentBase<uint, UserInfo>
    {
        public HashSet<string> ReservedUserNames { get; set; }

        internal UserDataService() { }
    }
}

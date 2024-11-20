using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOHTTPS.Protocol.Data
{
    internal record UserAccount(string UserName, string Password)
    {
        public IEnumerable<uint> AvatarIDs { get; } = new List<uint>();
    }
}

using nio2so.DataService.Common.Types.Lot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.DataService.Common.Types
{
    public class LotInfo
    {
        public LotInfo() { }

        public LotProfile Profile { get; set; } = new();
    }
}

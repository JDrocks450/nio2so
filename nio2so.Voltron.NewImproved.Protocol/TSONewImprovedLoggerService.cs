using nio2so.Voltron.Core.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol
{
    public class TSONewImprovedLoggerService : TSOLoggerServiceBase
    {
        public TSONewImprovedLoggerService(string? SysLogPath = null) : base(SysLogPath) { }
    }
}

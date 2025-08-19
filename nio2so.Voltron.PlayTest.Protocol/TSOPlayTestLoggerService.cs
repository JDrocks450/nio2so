using nio2so.Voltron.Core.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PlayTest.Protocol
{
    public class TSOPlayTestLoggerService : TSOLoggerServiceBase
    {
        public TSOPlayTestLoggerService(string? SysLogPath = null) : base(SysLogPath) { }
    }
}

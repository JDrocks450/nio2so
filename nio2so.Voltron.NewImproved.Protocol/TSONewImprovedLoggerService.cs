using nio2so.Voltron.Core.Telemetry;

namespace nio2so.Voltron.NewImproved.Protocol
{
    public class TSONewImprovedLoggerService : TSOLoggerServiceBase
    {
        public TSONewImprovedLoggerService(string? SysLogPath = null) : base(SysLogPath) { }
    }
}

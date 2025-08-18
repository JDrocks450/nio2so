using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;

namespace nio2so.Voltron.Core
{
    /// <summary>
    /// Base interface for Voltron Servers
    /// </summary>
    public interface ITSOServer
    {
        /// <summary>
        /// Additional services that <see cref="TSORegulator"/> objects can use for their functionality
        /// </summary>
        TSOServerServiceManager Services { get; }
        TSOLoggerServiceBase Logger { get; }
        TSORegulatorManager Regulators { get; }
        string Name { get; }
        public bool IsRunning { get; set; }
    }
}

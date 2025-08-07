using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO;

namespace nio2so.TSOTCP.Voltron.Protocol
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
        TSOServerTelemetryServer Telemetry { get; }
        TSORegulatorManager Regulators { get; }
        
        public bool IsRunning { get; set; }
    }
}

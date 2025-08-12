using nio2so.Data.Common.Testing;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace nio2so.DataService.Common.Types
{
    /// <summary>
    /// Settings for the nio2so Voltron Server
    /// </summary>
    public record VoltronServerSettings
    {
        public string ShardName { get; set; } = TestingConstraints.MyShardName;
        /// <summary>
        /// The Address the server will be listening on -- default is <c>localhost</c>
        /// </summary>
        public string ServerIPAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// The Port the server will be listening on -- default is <see cref="TestingConstraints.City_ListenPort"/>
        /// </summary>
        public int ServerPort { get; set; } = TestingConstraints.City_ListenPort;
        /// <summary>
        /// The maximum amount of connected clients at a given time
        /// </summary>
        public uint MaxConcurrentConnections { get; set; } = 15;

        /// <summary>
        /// Formatted as follows: 
        /// <c>{<see cref="ServerIPAddress"/>}:{<see cref="ServerPort"/>.ToString().Remove(2)}</c>
        /// </summary>
        [JsonIgnore]
        public string ServerConnectionAddress => $"{ServerIPAddress}:{ServerPort.ToString().Remove(2)}";
    }
}

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
        /// <summary>
        /// The default settings for a <see cref="VoltronServerSettings"/> instance
        /// </summary>
        public static VoltronServerSettings Default => new();
        /// <summary>
        /// The name of this Voltron Server shard -- default is <see cref="TestingConstraints.MyShardName"/>
        /// </summary>
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
        /// The size of the buffer that will be allocated for each client connection to Voltron for sending and receiving packets
        /// </summary>
        public ushort TSOAriesClientBufferLength { get; set; } = 0x200;
        /// <summary>
        /// Required certificate signing for builds after The Sims Online Pre-Alpha, not required for Pre-Alpha only
        /// </summary>
        public bool EnableSSL { get; set; } = false;

        /// <summary>
        /// Formatted as follows: 
        /// <c>{<see cref="ServerIPAddress"/>}:{<see cref="ServerPort"/>.ToString().Remove(2)}</c>
        /// </summary>
        [JsonIgnore]
        public string ServerConnectionAddress => $"{ServerIPAddress}:{ServerPort.ToString().Remove(2)}";
    }
}

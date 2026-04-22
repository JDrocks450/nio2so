using nio2so.Voltron.Core.TSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.ClientOnlinePDU)]
    public class TSOClientOnlinePDU : TSOVoltronPacket
    {
        /// <summary>
        /// m_pVersionInfo
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public record VersionInfo
        {
            public byte m_majorVersion { get; set; }
            public byte m_minorVersion { get; set; }
            public byte m_pointVersion { get; set; }
            public byte m_artVersion { get; set; }
        }

        /// <summary>
        /// m_pRelogStats
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public record RelogStats
        {
            public uint Unknown1 { get; set; } // 4 bytes

            // A 4-byte Unix timestamp with the most significant bit flipped
            // Likely will need to XOR this with 0x80000000 to get the real Unix time
            public uint m_time { get; set; }

            public ushort m_numberOfAttempts { get; set; } // 2-byte unsigned integer
        }

        /// <summary>
        /// m_pClientStatus
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public record ClientStatus
        {
            public uint m_lastExitCode { get; set; }     // 4-byte unsigned integer
            public byte m_lastFailureType { get; set; }
            public byte m_failureCount { get; set; }
            public bool m_isRunning { get; set; }        // 1 byte (0 or 1)
            public bool m_isRelogging { get; set; }     // 1 byte (0 or 1)
            public bool Unknown2 { get; set; }          // 1 byte (0 or 1)
        }

        public TSOClientOnlinePDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOClientOnlinePDU(VersionInfo clientVersion, RelogStats reloggingStats, ClientStatus status)
        {
            ClientVersion = clientVersion;
            ReloggingStats = reloggingStats;
            Status = status;
        }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.ClientOnlinePDU;

        public VersionInfo ClientVersion { get; set; }
        public RelogStats ReloggingStats { get; set; }
        public ClientStatus Status { get; set; }
    }
}

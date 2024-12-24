using MiscUtil.Conversion;
using nio2so.TSOTCP.City.TSO.Aries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    internal class TSOClientOnlinePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU;
        /// <summary>
        /// This will call <see cref="TSOVoltronPacket.MakeBodyFromProperties"/> for you in case you're using an initializer list. You're welcome.
        /// </summary>
        public TSOClientOnlinePDU()
        {
            MakeBodyFromProperties();
        }
        public TSOClientOnlinePDU(byte majorVersion,
                                  byte minorVersion,
                                  byte pointVersion,
                                  byte artVersion,
                                  uint arg1,
                                  DateTime time,
                                  uint numberOfAttempts,
                                  uint lastExitCode,
                                  byte lastFailureType,
                                  byte failureCount,
                                  byte isRunning,
                                  byte isRelogging,
                                  byte arg2)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            PointVersion = pointVersion;
            ArtVersion = artVersion;
            Arg1 = arg1;
            Time = time;
            NumberOfAttempts = numberOfAttempts;
            LastExitCode = lastExitCode;
            LastFailureType = lastFailureType;
            FailureCount = failureCount;
            IsRunning = isRunning;
            IsRelogging = isRelogging;
            Arg2 = arg2;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// m_pVersionInfo
        /// </summary>
        public byte MajorVersion { get; set; }
        /// <summary>
        /// m_pVersionInfo
        /// </summary>
        public byte MinorVersion { get; set; }
        /// <summary>
        /// m_pVersionInfo
        /// </summary>
        public byte PointVersion { get; set; }
        /// <summary>
        /// m_pVersionInfo
        /// </summary>
        public byte ArtVersion { get; set; }
        /// <summary>
        /// m_pRelogStats - Unknown
        /// </summary>
        public UInt32 Arg1 { get; set; }
        /// <summary>
        /// m_pRelogStats - Time Component
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// m_pRelogStats - Amount of attempts to relog in
        /// </summary>
        public UInt32 NumberOfAttempts { get; set; }
        /// <summary>
        /// m_pClientStatus - m_lastExitCode 
        /// </summary>
        public UInt32 LastExitCode { get; set; }
        /// <summary>
        /// m_pClientStatus - m_lastFailureType 
        /// </summary>
        public byte LastFailureType { get; set; }
        /// <summary>
        /// m_pClientStatus - m_failureCount 
        /// </summary>
        public byte FailureCount { get; set; }
        /// <summary>
        /// m_pClientStatus - m_isRunning 
        /// </summary>
        public byte IsRunning { get; set; }
        /// <summary>
        /// m_pClientStatus - m_isRelogging 
        /// </summary>
        public byte IsRelogging { get; set; }
        /// <summary>
        /// m_pClientStatus - Unknown
        /// </summary>
        public byte Arg2 { get; set; }
    }
}

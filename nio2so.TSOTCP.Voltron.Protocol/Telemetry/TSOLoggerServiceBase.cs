using nio2so.Data.Common.Testing;
using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using QuazarAPI;
using System.Diagnostics;

namespace nio2so.Voltron.Core.Telemetry
{
    /// <summary>
    /// A predefined <see cref="TSOLoggerServiceBase"/> that logs to the console and optionally to a system log file.
    /// </summary>
    public sealed class TSOVoltronBasicLogger : TSOLoggerServiceBase
    {
        public TSOVoltronBasicLogger(string? SysLogPath = null) : base(SysLogPath) { }
    }

    /// <summary>
    /// A pipeline to receive information on the current running state of a <see cref="ITSOServer"/>
    /// </summary>
    public abstract class TSOLoggerServiceBase : ITSOService
    {
        public const ushort CONSOLE_WIDTH = 50;

        public enum LogSeverity
        {
            /// <summary>
            /// A general message -- no severity
            /// </summary>
            Message,
            /// <summary>
            /// Something that should have attention but isn't critical
            /// </summary>
            Warnings,
            /// <summary>
            /// New communications not yet documented/handled
            /// </summary>
            Discoveries,
            /// <summary>
            /// Errors that are non-fatal but of high importance
            /// </summary>
            Errors,
            /// <summary>
            /// Emergencies only
            /// </summary>
            Fatal,
            /// <summary>
            /// A log from the <see cref="QuazarAPI.Networking.Standard.QuazarServer{T}"/> TCP transport layer
            /// <para>The severity for these is not known</para>
            /// </summary>
            TCPLayer,
            TCPLayer_Errors
        }
        /// <summary>
        /// Contains information about a log entry that is written to the console.
        /// </summary>
        /// <param name="Severity">The console log type</param>
        /// <param name="Sender">The sender component</param>
        /// <param name="Content">The content of the message</param>
        /// <param name="Time">The time this event occurred, default is <see cref="DateTime.Now"/></param>
        public record ConsoleLogEntry(LogSeverity Severity, string Sender, string Content, DateTime? Time = null);

        public string ServerName => Parent?.Name ?? GetType().Name;

        private Stopwatch? _groupWatch;
        private string? _systemLogPath;
        public bool IsSysLogging => _systemLogPath != null;

        public ITSOServer Parent { get; set; }

        /// <summary>
        /// Creates a new <see cref="TSOLoggerServiceBase"/> instance.
        /// </summary>
        /// <param name="SysLogPath"></param>
        protected TSOLoggerServiceBase(string? SysLogPath = null)
        {
            _systemLogPath = SysLogPath;
        }
        /// <summary>
        /// Initializes the <see cref="TSOLoggerServiceBase"/> with the given <paramref name="Server"/>
        /// </summary>
        /// <param name="Server"></param>
        public virtual void Init(ITSOServer Server)
        {
            QConsole.OnLogUpdated += (ChannelName, Raw, Formatted) =>
            {
                OnConsoleLog(new(LogSeverity.TCPLayer, ChannelName, Raw));
            };
            QConsole.SetConsoleMode(false);
            QConsole.WriteLine("TelemetryServer", "Init complete.");
            Log($"\n****** LOG STARTED {DateTime.Now.ToString()} ******\n");          
        }
        /// <summary>
        /// Fired when a new <see cref="TSOTCPPacket"/> is received or sent
        /// </summary>
        /// <param name="Direction"></param>
        /// <param name="Time"></param>
        /// <param name="Packet"></param>
        /// <param name="PDUEnclosed"></param>
        public virtual void OnAriesPacket(NetworkTrafficDirections Direction, DateTime Time, TSOTCPPacket Packet, TSOVoltronPacket? PDUEnclosed = default)
        {
            if (!TestingConstraints.VerboseLogging) return;

            Console.ForegroundColor = ConsoleColor.Magenta;

            string? PDUName = PDUEnclosed != null ? Parent.Services.Get<TSOPDUFactoryServiceBase>().GetVoltronPacketTypeName(PDUEnclosed.VoltronPacketType) : "Unknown";
            Log($"{Time.ToLongTimeString()} - *ARIES* [{Direction}] {(TSOAriesPacketTypes)Packet.PacketType}" +
                $"{(PDUEnclosed != null ? " containing: " : "")}{PDUEnclosed?.ToShortString()}");

            //**LOG PDU TO DISK**
            Packet.WritePacketToDisk(Direction == NetworkTrafficDirections.INBOUND, PDUName);
        }
        /// <summary>
        /// Fired when a new <see cref="TSOVoltronPacket"/> is received or sent
        /// </summary>
        /// <param name="Direction"></param>
        /// <param name="Time"></param>
        /// <param name="PDU"></param>
        /// <param name="ClientID"></param>
        public virtual void OnVoltronPacket(NetworkTrafficDirections Direction, DateTime Time, TSOVoltronPacket PDU, uint? ClientID = null)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.Green,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.Cyan,
                _ => ConsoleColor.Blue
            };            

            Log($"{Time.ToLongTimeString()} - *VOLTRON* [{Direction}] {PDU.ToShortString()}");

            //**LOG PDU TO DISK
            if (Direction == NetworkTrafficDirections.INBOUND || Direction == NetworkTrafficDirections.OUTBOUND)
                PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }
        /// <summary>
        /// Fired when a <see cref="TSOVoltronPacket"/> is received that is not yet documented in this protocol
        /// </summary>
        /// <param name="PacketType"></param>
        /// <param name="DataLength"></param>
        /// <param name="ClientID"></param>
        public virtual void OnVoltron_OnDiscoveryPacket(ushort PacketType, int DataLength)
        {
            var displayName = Parent.Services.Get<TSOPDUFactoryServiceBase>().GetVoltronPacketTypeName(PacketType) ?? $"Unknown Packet Type: 0x{PacketType:X4}";

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Log($"TSO PDU Discovery ***********\n");
            Log($"Found the {displayName} PDU with: {DataLength} bytes. This will be saved to the discoveries folder.");
            Log($"\n****************************");
        }
        /// <summary>
        /// Fired when a new <see cref="ConsoleLogEntry"/> is received
        /// </summary>
        /// <param name="Entry"></param>
        protected virtual void OnConsoleLog(ConsoleLogEntry Entry)
        {
            Console.ForegroundColor = Entry.Severity switch
            {
                LogSeverity.Message => ConsoleColor.Yellow,
                LogSeverity.TCPLayer => ConsoleColor.White,
                LogSeverity.TCPLayer_Errors => ConsoleColor.Red,
                LogSeverity.Warnings => ConsoleColor.DarkYellow,
                LogSeverity.Errors => ConsoleColor.Red,
                _ => ConsoleColor.White,
            };

            if (!TestingConstraints.VerboseLogging && Entry.Severity == LogSeverity.TCPLayer) return;

            //**PRETTY PRINT FOR MULTILINE
            var time = Entry.Time ?? DateTime.Now;
            var message = $"{Entry.Content}";
            var header = $"--- {time} ";
            var nmessage = Entry.Content.Contains('\n') ?
                $"{header}{new string('-', CONSOLE_WIDTH - header.Length)} \n" +
                $"[{Entry.Sender}]: {message}\n" +
                $"{new string('-', CONSOLE_WIDTH)} \n"
                :
                $"{time.ToLongTimeString()} - {Entry.Severity} [{Entry.Sender}]: {message}";
            //**

            Log(nmessage);
        }
        /// <summary>
        /// Logs the given <see cref="ConsoleLogEntry"/> to the console and, if enabled, to the system log file.
        /// </summary>
        /// <param name="Entry"></param>
        public void LogConsole(ConsoleLogEntry Entry) => OnConsoleLog(Entry);
        /// <summary>
        /// Internal method to log a message to the console and, if enabled, to the system log file.
        /// </summary>
        /// <param name="Message"></param>
        protected void Log(string Message)
        {
            Message = $"[*THREAD: {Thread.CurrentThread.ManagedThreadId}*] {ServerName} - {Message}";
            Console.WriteLine(Message);
            if (!IsSysLogging) return;
            lock (this)
            {
                using (FileStream fs = File.OpenWrite(_systemLogPath))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(Message);
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}

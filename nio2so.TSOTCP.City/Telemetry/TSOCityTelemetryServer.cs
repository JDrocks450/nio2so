using nio2so.Formats.DB;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.Telemetry
{
    /// <summary>
    /// A pipeline to receive information on the current running state of a <see cref="TSOCityServer"/>
    /// </summary>
    public class TSOCityTelemetryServer
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
            TCPLayer
        }

        public record ConsoleLogEntry(LogSeverity Severity, string Sender, string Content, DateTime? Time = null);

        private readonly TSOCityServer _parent;
        private string? _systemLogPath;
        public bool IsSysLogging => _systemLogPath != null;

        /// <summary>
        /// Global telemetry instance. Really should not let this be permanent
        /// </summary>
        public static TSOCityTelemetryServer Global { get; private set; }

        internal TSOCityTelemetryServer(TSOCityServer Parent, string? SysLogPath = null)
        {
            _parent = Parent;
            _systemLogPath = SysLogPath;            

            Init();
        }

        private void Init()
        {
            QConsole.OnLogUpdated += (string ChannelName, string Raw, string Formatted) =>
            {
                OnConsoleLog(new(LogSeverity.TCPLayer, ChannelName, Raw));
            };
            QConsole.SetConsoleMode(false);
            QConsole.WriteLine("TelemetryServer", "Init complete.");
            Log($"****** LOG STARTED {DateTime.Now.ToString()} ******");

            Global = this; // ugh
        }

        internal void OnAriesPacket(NetworkTrafficDirections Direction, DateTime Time, TSOTCPPacket Packet, TSOVoltronPacket? PDUEnclosed = default)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            string? PDUName = PDUEnclosed?.KnownPacketType.ToString();
            Console.WriteLine($"{Time.ToLongTimeString()} - *ARIES* [{Direction}] {(TSOAriesPacketTypes)Packet.PacketType}" +
                $"{(PDUEnclosed != null ? " containing: " : "")}{PDUEnclosed?.ToShortString()}");

            //**LOG PDU TO DISK**
            Packet.WritePacketToDisk(Direction == NetworkTrafficDirections.INBOUND, PDUName);
        }

        internal void OnVoltronPacket(NetworkTrafficDirections Direction, DateTime Time, TSOVoltronPacket PDU, uint? ClientID = null)
        {
            //**Auto-Redirect**
            if (PDU is TSODBRequestWrapper dbWrapper)
            {
                OnVoltron_DBWrapperPDU(Direction, Time, dbWrapper, ClientID);
                return;
            }

            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.Green,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.Cyan  ,
                _ => ConsoleColor.Blue
            };
            Console.WriteLine($"{Time.ToLongTimeString()} - *VOLTRON* [{Direction}] {PDU.ToShortString()}");

            //**LOG PDU TO DISK
            PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }

        internal void OnVoltron_DBWrapperPDU(NetworkTrafficDirections Direction, DateTime Time, TSODBRequestWrapper PDU, uint? ClientID = null)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.Green,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.Cyan,
                _ => ConsoleColor.Cyan
            };
            Console.WriteLine($"{Time.ToLongTimeString()} - *VOLTRON_DATABASE* [{Direction}] {PDU.ToShortString()}");

            //**LOG PDU TO DISK
            PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }

        internal void OnVoltron_OnDiscoveryPacket(TSOVoltronPacket DiscoveredPacket, uint? ClientID = null)
        {
            bool written = TSOPDUFactory.LogDiscoveryPacketToDisk(DiscoveredPacket.VoltronPacketType, DiscoveredPacket.Body);
            var displayName = "0x" + DiscoveredPacket.VoltronPacketType.ToString("X4");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"TSO PDU Discovery ***********\n");
            if (written)            
                Console.WriteLine($"Discovered the {displayName} PDU with: {DiscoveredPacket.BodyLength} bytes. Add it to constants!");            
            else
                Console.WriteLine($"Found the {displayName} PDU with: {DiscoveredPacket.BodyLength} bytes. Make a class for it. ");
            Console.WriteLine($"\n****************************");
        }

        internal void OnConsoleLog(ConsoleLogEntry Entry)
        {
            Console.ForegroundColor = Entry.Severity switch
            {
                LogSeverity.Message => ConsoleColor.Yellow,
                LogSeverity.TCPLayer => ConsoleColor.White,
                LogSeverity.Warnings => ConsoleColor.DarkYellow,
                LogSeverity.Errors => ConsoleColor.Red,
                _ => ConsoleColor.White,
            };

            //**PRETTY PRINT FOR MULTILINE
            var time = (Entry.Time ?? DateTime.Now);
            var message = Entry.Content;
            var header = $"--- {time} ";
            var nmessage = (Entry.Content.Contains('\n')) ?
                $"{header}{new string('-', CONSOLE_WIDTH - header.Length)} \n" +
                $"[{Entry.Sender}]: {message}\n" +
                $"{new string('-', CONSOLE_WIDTH)} \n"
                :
                $"{time.ToLongTimeString()} - {Entry.Severity} [{Entry.Sender}]: {message}";
            //**

            Console.WriteLine(nmessage);
        }

        internal static void LogConsole(ConsoleLogEntry Entry) => Global.OnConsoleLog(Entry);

        internal void OnHouseBlob(NetworkTrafficDirections Direction, uint HouseID, TSODBHouseBlob houseBlob)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.DarkCyan,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.DarkCyan,
                _ => ConsoleColor.Cyan
            };
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - *HOUSE_BLOB* [{Direction}] HouseID: {HouseID} Size: {houseBlob.BlobData.Length}");

            TSOHouseFactory.SetHouseBlobByIDToDisk(HouseID, houseBlob);
        }

        private void Log(string Message)
        {
            Console.WriteLine(Message);
            if (!IsSysLogging) return;
            File.WriteAllLines(_systemLogPath, new[] { Message });
        }
    }
}

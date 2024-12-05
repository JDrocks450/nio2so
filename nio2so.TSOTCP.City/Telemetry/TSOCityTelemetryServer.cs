using nio2so.Formats.DB;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Stopwatch _groupWatch;
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
            Log($"\n****** LOG STARTED {DateTime.Now.ToString()} ******\n");

            Global = this; // ugh            
        }

        internal void OnAriesPacket(NetworkTrafficDirections Direction, DateTime Time, TSOTCPPacket Packet, TSOVoltronPacket? PDUEnclosed = default)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            string? PDUName = PDUEnclosed?.KnownPacketType.ToString();
            Log($"{Time.ToLongTimeString()} - *ARIES* [{Direction}] {(TSOAriesPacketTypes)Packet.PacketType}" +
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
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.Cyan,
                _ => ConsoleColor.Blue
            };
            Log($"{Time.ToLongTimeString()} - *VOLTRON* [{Direction}] {PDU.ToShortString()}");

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
            Log($"{Time.ToLongTimeString()} - *VOLTRON_DATABASE* [{Direction}] {PDU.ToShortString()}");

            //**LOG PDU TO DISK
            PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }

        internal void OnVoltron_OnDiscoveryPacket(ushort PacketType, byte[] PacketData, uint? ClientID = null)
        {
            bool written = TSOPDUFactory.LogDiscoveryPacketToDisk(PacketType, PacketData);
            var displayName = Enum.IsDefined((TSO_PreAlpha_VoltronPacketTypes)PacketType) ?
                ((TSO_PreAlpha_VoltronPacketTypes)PacketType).ToString() :
                "0x" + PacketType.ToString("X4");

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Log($"TSO PDU Discovery ***********\n");
            if (written)
                Log($"Discovered the {displayName} PDU with: {PacketData.Length} bytes. Dumped to Discoveries.");
            else
                Log($"Found the {displayName} PDU with: {PacketData.Length} bytes. You already have a copy of that one.");
            Log($"\n****************************");
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

            Log(nmessage);
        }

        internal static void LogConsole(ConsoleLogEntry Entry) => Global.OnConsoleLog(Entry);

        internal void OnHouseBlob(NetworkTrafficDirections Direction, uint HouseID, TSODBHouseBlob houseBlob)
        {
            OnBlobBase(Direction, HouseID, houseBlob);
            if (Direction == NetworkTrafficDirections.INBOUND)
                TSOFactoryBase.Get<TSOHouseFactory>().SetHouseBlobByIDToDisk(HouseID, houseBlob);
        }
        internal void OnCharBlob(NetworkTrafficDirections Direction, uint AvatarID, TSODBCharBlob charBlob)
        {
            OnBlobBase(Direction, AvatarID, charBlob);
            if (Direction == NetworkTrafficDirections.INBOUND)
                TSOFactoryBase.Get<TSOAvatarFactory>().SetCharBlobByIDToDisk(AvatarID, charBlob);
        }
        private void OnBlobBase(NetworkTrafficDirections Direction, uint ID, TSODBBlob Blob)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.DarkCyan,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.DarkCyan,
                _ => ConsoleColor.Cyan
            };
            Log($"{DateTime.Now.ToLongTimeString()} - *{Blob.GetType().Name}* [{Direction}] ObjectID: {ID} Size: {Blob.BlobData.Length}");
        }
        internal void OnCharData(NetworkTrafficDirections Direction, uint AvatarID, TSODBChar CharData)
        {
            OnBlobBase(Direction, AvatarID, CharData);
            if (Direction == NetworkTrafficDirections.INBOUND)
                TSOFactoryBase.Get<TSOAvatarFactory>().SetCharByIDToDisk(AvatarID, CharData);
        }

        private void Log(string Message)
        {
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
    }
}

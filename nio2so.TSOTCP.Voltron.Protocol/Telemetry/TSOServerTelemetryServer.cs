using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Aries;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using QuazarAPI;
using System.Diagnostics;

namespace nio2so.TSOTCP.Voltron.Protocol.Telemetry
{
    /// <summary>
    /// A pipeline to receive information on the current running state of a <see cref="TSOCityServer"/>
    /// </summary>
    public class TSOServerTelemetryServer
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

        public record ConsoleLogEntry(LogSeverity Severity, string Sender, string Content, DateTime? Time = null);

        public string ServerName { get; }

        private Stopwatch? _groupWatch;
        private string? _systemLogPath;
        public bool IsSysLogging => _systemLogPath != null;

        /// <summary>
        /// Global telemetry instance. Really should not let this be permanent
        /// </summary>
        public static TSOServerTelemetryServer Global { get; private set; }

        public TSOServerTelemetryServer(string ServerName, string? SysLogPath = null)
        {
            this.ServerName = ServerName;
            _systemLogPath = SysLogPath;

            Init();
        }

        private void Init()
        {
            QConsole.OnLogUpdated += (ChannelName, Raw, Formatted) =>
            {
                OnConsoleLog(new(LogSeverity.TCPLayer, ChannelName, Raw));
            };
            QConsole.SetConsoleMode(false);
            QConsole.WriteLine("TelemetryServer", "Init complete.");
            Log($"\n****** LOG STARTED {DateTime.Now.ToString()} ******\n");

            Global = this; // ugh            
        }

        public void OnAriesPacket(NetworkTrafficDirections Direction, DateTime Time, TSOTCPPacket Packet, TSOVoltronPacket? PDUEnclosed = default)
        {
            if (!TestingConstraints.VerboseLogging) return;

            Console.ForegroundColor = ConsoleColor.Magenta;

            string? PDUName = PDUEnclosed?.KnownPacketType.ToString();
            Log($"{Time.ToLongTimeString()} - *ARIES* [{Direction}] {(TSOAriesPacketTypes)Packet.PacketType}" +
                $"{(PDUEnclosed != null ? " containing: " : "")}{PDUEnclosed?.ToShortString()}");

            //**LOG PDU TO DISK**
            Packet.WritePacketToDisk(Direction == NetworkTrafficDirections.INBOUND, PDUName);
        }

        public void OnVoltronPacket(NetworkTrafficDirections Direction, DateTime Time, TSOVoltronPacket PDU, uint? ClientID = null)
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
            if (!TestingConstraints.VerboseLogging)
            {
                if (PDU is TSOSplitBufferPDU)
                    return; // skip these
                if (PDU is ITSODataBlobPDU standardMsg &&
                    standardMsg.DataBlobContentObject.GetAs<TSOStandardMessageContent>().
                    Match(TSO_PreAlpha_MasterConstantsTable.kServerTickConfirmationMsg))
                    return; // this is a server confirmation message, they spam. Do not log these.
            }

            Log($"{Time.ToLongTimeString()} - *VOLTRON* [{Direction}] {PDU.ToShortString()}");

            //**LOG PDU TO DISK
            if (Direction == NetworkTrafficDirections.INBOUND || Direction == NetworkTrafficDirections.OUTBOUND)
                PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }

        public void OnVoltron_DBWrapperPDU(NetworkTrafficDirections Direction, DateTime Time, TSODBRequestWrapper PDU, uint? ClientID = null)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.Green,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.Cyan,
                _ => ConsoleColor.Cyan
            };
            Log($"{Time.ToLongTimeString()} - *VOLTRON_DATABASE* [{Direction}] {PDU.ToShortString()}");

            //**LOG PDU TO DISK
            if (Direction == NetworkTrafficDirections.INBOUND || Direction == NetworkTrafficDirections.OUTBOUND)
                PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }

        public void OnVoltron_OnDiscoveryPacket(ushort PacketType, byte[] PacketData, uint? ClientID = null)
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

        public void OnConsoleLog(ConsoleLogEntry Entry)
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
            var message = Entry.Content;
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

        public static void LogConsole(ConsoleLogEntry Entry)
        {
            if (Global == null)
            {
                Debug.WriteLine($"Cannot log as there is no active {nameof(TSOServerTelemetryServer)}!");
                return;
            }
            Global.OnConsoleLog(Entry);
        }

        public void OnHouseBlob(NetworkTrafficDirections Direction, uint HouseID, TSODBHouseBlob HouseBlob)
        {
            OnBlobBase(Direction, HouseID, "House File Stream");
            if (Direction == NetworkTrafficDirections.INBOUND)
                TSOFactoryBase.Get<TSOHouseFactory>().SetHouseBlobByIDToDisk(HouseID, HouseBlob);
        }
        public void OnCharBlob(NetworkTrafficDirections Direction, uint AvatarID, TSODBCharBlob charBlob)
        {
            OnBlobBase(Direction, AvatarID, "Character File Stream");
            if (Direction == NetworkTrafficDirections.INBOUND)
                TSOFactoryBase.Get<TSOAvatarFactory>().SetCharBlobByIDToDisk(AvatarID, charBlob);
        }
        private void OnBlobBase(NetworkTrafficDirections Direction, uint ID, string Type)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.DarkCyan,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.DarkCyan,
                _ => ConsoleColor.Cyan
            };
            Log($"{DateTime.Now.ToLongTimeString()} - *{Type}* [{Direction}] ObjectID: {ID}");
        }
        public void OnCharData(NetworkTrafficDirections Direction, uint AvatarID, TSODBChar CharData)
        {
            OnBlobBase(Direction, AvatarID, "Character Profile");
            if (Direction == NetworkTrafficDirections.INBOUND)
                TSOFactoryBase.Get<TSOAvatarFactory>().SetCharByIDToDisk(AvatarID, CharData);
        }

        private void Log(string Message)
        {
            Message = $"{ServerName} - {Message}";
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

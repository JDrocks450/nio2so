using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron;
using QuazarAPI;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Aries
{
    /// <summary>
    /// <see href="http://wiki.niotso.org/Maxis_Protocol#AriesPackets"/>    
    /// </summary>
    public enum TSOAriesPacketTypes : uint
    {
        Voltron = 0,
        Disconnect = 1,
        InitializeConnection = 3,
        Client_SessionInfoResponse = 21,
        ClientSessionInfo = 22,
        Unknown = 26,
        ProcTimingPing = 27,
        InitializePerformanceMon = 28,
        Client_PerformanceMonInitResponse = 29,
        RelogonStart = 31,
        RelogonComplete = 44
    }

    /// <summary>
    /// An Aries Packet.
    /// </summary>
    public class TSOTCPPacket : QuazarAPI.Networking.Data.PacketBase
    {
        private static bool _warningShownOnce = false;

        public override uint GetHeaderSize() => ARIES_FRAME_HEADER_LEN;

        /// <summary>
        /// Aries Packet Type <para/>
        /// See <see cref="TSOAriesPacketTypes"/> for known Packet Types for TSO.        
        /// </summary>
        public uint PacketType { get; set; } = (uint)TSOAriesPacketTypes.Voltron;
        /// <summary>
        /// Useless as it is not programmed in a substantial or usable way
        /// <para><see href="http://wiki.niotso.org/Maxis_Protocol#AriesPackets"/></para>
        /// </summary>
        public uint Timestamp { get; set; } = 0;

        /// <summary>
        /// See: <see cref="PayloadSize"/>
        /// </summary>
        public override uint BodyLength => PayloadSize;
        /// <summary>
        /// The size of the following data. If not a split Aries frame, this should be equal to this packet's total size - 12
        /// </summary>
        public uint PayloadSize { get; set; } = 0;
        public Stream BodyStream => _bodyBuffer;

        public const byte ARIES_FRAME_HEADER_LEN = sizeof(uint) * 3;

        public TSOTCPPacket() : base() { }        
        public TSOTCPPacket(uint Type, uint Time, uint Size) : this()
        {
            PacketType = Type;
            Timestamp = Time;
            PayloadSize = Size;
            AllocateBody(Size);
        }
        public TSOTCPPacket(TSOAriesPacketTypes Type, uint Time, uint Size) : this((uint)Type, Time, Size) { }
        public TSOTCPPacket(uint Type, uint Time, byte[] PacketData) : this(Type, Time, (uint)PacketData.Length)
        {
            EmplaceBody(PacketData);
        }
        public TSOTCPPacket(TSOAriesPacketTypes Type, uint Time, byte[] PacketData) : this((uint)Type, Time, PacketData) { }

        /// <summary>
        /// Parses out one (or many) Aries packets from the response body
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public override IEnumerable<T> ParseAllPackets<T>(ref byte[] Data)
        {
            List<T> packets = new List<T>();            
            int currentIndex = 0;
            do
            {
                byte[] headerBytes = new byte[TSOTCPPacket.ARIES_FRAME_HEADER_LEN];
                Array.Copy(Data, currentIndex, headerBytes, 0, headerBytes.Length);
                var headerSuccess = TryGetAriesHeader(headerBytes, out uint pType, out uint time, out uint size);
                if (!headerSuccess) // THROW IF FIRST PACKET
                {
                    if (currentIndex == 0)
                        TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Errors, 
                            "cTSOTCPPacket", "First packet in the response body isn't an Aries packet.")); // FIRST PACKET ISN'T EVEN ARIES
                    break;
                }
                uint packetBuffer = size + ARIES_FRAME_HEADER_LEN;
                if(Data.Length - currentIndex < packetBuffer)
                    throw new InternalBufferOverflowException("Packet length reported by client is longer than the received bytes");
                byte[] frameData = new byte[packetBuffer];
                Array.Copy(Data, currentIndex, frameData, 0, frameData.Length);
                T val = ParsePacket<T>(frameData, out int readBytes);
                if (val == default(T)) break;
                currentIndex += readBytes;
                packets.Add(val);
            }
            while(currentIndex < Data.Length);
            return packets; // RETURN ALL PACKETS FOUND IN THE RESPONSE BODY
        }
        /// <summary>
        /// Parses out an Aries packet from the response body
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public override T ParsePacket<T>(byte[] bytes, out int endIndex)
        {
            endIndex = bytes.Length;
            var headerSuccess = TryGetAriesHeader(bytes, out uint pType, out uint time, out uint size);
            if (!headerSuccess) return default(T);
            byte[] bodyArray = new byte[size];
            Array.Copy(bytes, ARIES_FRAME_HEADER_LEN, bodyArray, 0, (int)size);
            return new TSOTCPPacket(pType, time, bodyArray) as T;
        }        

        public override byte[] GetBytes()
        {
            byte[] packetData = new byte[PayloadSize + ARIES_FRAME_HEADER_LEN];
            Array.Copy(BitConverter.GetBytes(PacketType), 0, packetData, sizeof(uint) * 0, sizeof(uint));
            if (Timestamp == 0)
            {
                Timestamp = 0xA1A2A3A4;
                if (!_warningShownOnce)
                    TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Warnings,
                        "cTSOTCPPacket", "Timestamp parameter was 0x0. Changed to 0xA1A2A3A4 just in time. " +
                        "This warning will not be shown again."));
                _warningShownOnce = true;
            }
            Array.Copy(BitConverter.GetBytes(Timestamp), 0, packetData, sizeof(uint) * 1, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(PayloadSize), 0, packetData, sizeof(uint) * 2, sizeof(uint));
            if (PayloadSize > 0) {
                byte[] bytes = base.GetBytes();
                Array.Copy(bytes, 0, packetData, ARIES_FRAME_HEADER_LEN, PayloadSize);
            }
            return packetData;
        }

        public override bool TryGetHeaderData(in Byte[] Buffer, out uint Size)
        {
            Size = 0;
            if (Buffer == null) throw new NullReferenceException();
            if (Buffer.Length < ARIES_FRAME_HEADER_LEN) return false;
            return TryGetAriesHeader(Buffer, out _, out _, out Size);
        }
        /// <summary>
        /// Attempts to read the Aries Packet Header.
        /// <para>Make sure to handle exceptions yourself, or use <see cref="TryGetAriesHeader(byte[], out uint, out uint, out uint)"/></para>
        /// </summary>
        /// <param name="PacketData"></param>
        /// <param name="PacketType"></param>
        /// <param name="Timestamp"></param>
        /// <param name="PayloadSize"></param>
        /// <exception cref="ArgumentException"></exception>
        protected static void GetAriesHeader(byte[] PacketData, out uint PacketType, out uint Timestamp, out uint PayloadSize)
        {
            if (PacketData.Length < ARIES_FRAME_HEADER_LEN)
                throw new ArgumentException($"{nameof(PacketData)} is less than {ARIES_FRAME_HEADER_LEN}!");
            int paramIndex = 0;
            //THESE FUNCTIONS CAN THROW EXCEPTIONS. BE CAREFUL WHEN USING IT. 
            PacketType = BitConverter.ToUInt32(PacketData, sizeof(uint) * paramIndex++);
            Timestamp = BitConverter.ToUInt32(PacketData, sizeof(uint) * paramIndex++);
            PayloadSize = BitConverter.ToUInt32(PacketData, sizeof(uint) * paramIndex++);
        }
        protected static bool TryGetAriesHeader(byte[] PacketData, out uint PacketType, out uint Timestamp, out uint PayloadSize)
        {
            PacketType = Timestamp = PayloadSize = 0;
            try
            {
                GetAriesHeader(PacketData, out PacketType, out Timestamp, out PayloadSize);                
            }
            catch (Exception e)
            { // TRY FUNCTION WILL IGNORE ERRORS
                TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Errors, 
                    "cTSOTCPPacket", $"Error when parsing Aries header: {e.Message}"));
                return false;
            }
            return PayloadSize != 0;
        }

        public static void WriteAllPacketsToDisk(IEnumerable<TSOTCPPacket> packets, string Directory = "/packets/tsotcppackets/")
        {
            int packetNumber = 0;
            System.IO.Directory.CreateDirectory(Directory);
            foreach (TSOTCPPacket packet in packets)
            {
                string myName = $"[{packetNumber}] {(TSOAriesPacketTypes)packet.PacketType} Packet - {DateTime.Now.ToShortTimeString()}.dat";
                File.WriteAllBytes(Path.Combine(Directory, myName), packet.GetBytes());
            }
        }

        /// <summary>
        /// Writes this <see cref="TSOTCPPacket"/>'s contents to the disk
        /// <para>You can use the <paramref name="PacketName"/> parameter to change what Packet Type appears in the brackets in the filename.</para>
        /// By <see langword="default"/>, this would be the <see cref="TSOAriesPacketTypes"/> <see cref="PacketType"/> name of this <see cref="TSOTCPPacket"/>
        /// </summary>
        /// <param name="Incoming">Dictates whether this <see cref="TSOTCPPacket"/> coming from the Server or from the local machine. 
        /// <see langword="true"/> for local machine.</param>
        /// <param name="PacketName">You can use the <paramref name="PacketName"/> parameter to change what Packet Type appears in the brackets in the filename.
        /// By <see langword="default"/>, this would be the <see cref="TSOAriesPacketTypes"/> <see cref="PacketType"/> name of this <see cref="TSOTCPPacket"/></param>
        /// <param name="Directory"></param>
        public void WritePacketToDisk(bool Incoming = true, string? PacketName = default, string Directory = TSOVoltronConst.AriesPacketDirectory)
        {
            if (!TestingConstraints.LogPackets) return;
            if (!TestingConstraints.LogAriesPackets) return;
            System.IO.Directory.CreateDirectory(Directory);
            var now = DateTime.Now;
            string myName = $"{(Incoming ? "IN" : "OUT")} [{PacketName ?? "Voltron"}] Packet {now.Hour%12},{now.Minute},{now.Second},{now.Nanosecond}.dat";
            File.WriteAllBytes(Path.Combine(Directory, myName), GetBytes());
        }
    }
}

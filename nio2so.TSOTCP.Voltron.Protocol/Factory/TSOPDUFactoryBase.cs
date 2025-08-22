using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Collections;
using nio2so.Voltron.Core.TSO.PDU;
using System.Reflection;

namespace nio2so.Voltron.Core.Factory
{
    public static class TSOPDUExtensions
    {
        public static string GetVoltronPacketTypeName(this TSOVoltronPacket packet, ITSOServer Server) => Server.Services.Get<TSOPDUFactoryServiceBase>().GetVoltronPacketTypeName(packet.VoltronPacketType);
    }

    /// <summary>
    /// This is an interface to use when reading/writing <see cref="TSOVoltronPacket"/>
    /// <para/>When derived, this class can be extended to handle special Voltron packets that are not of base type <see cref="TSOVoltronPacket"/>.
    /// </summary>
    public abstract class TSOPDUFactoryServiceBase : ITSOService
    {
        private Dictionary<uint, Type> typeMap = new();
        private HashSet<Assembly> _assemblies;

        public ITSOServer Parent
        {
            get; set;
        }

        public TSOPDUFactoryServiceBase(params Assembly[] Assemblies)
        {
            _assemblies = [.. Assemblies];
        }
        /// <summary>
        /// Maps the assemblies found in the list of assemblies passed to the constructor.
        /// </summary>
        /// <param name="Server"></param>
        public void Init(ITSOServer Server)
        {
            foreach (var assembly in _assemblies)
                MapAssembly(assembly);
        }
        /// <summary>
        /// Maps the given <paramref name="Assembly"/> to this factory.
        /// <para/>Will search for all <see cref="TSOVoltronPDU"/> attributes on classes in the assembly, and derived classes can specify additional attributes to map special Voltron packets.
        /// </summary>
        /// <param name="Assembly"></param>
        public void MapAssembly(Assembly Assembly)
        {
            LogConsole($"\nBeginning to map VoltronPacketType -> Classes in Assembly: {Assembly.FullName}\n", TSOLoggerServiceBase.LogSeverity.Warnings);
            foreach (var type in Assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<TSOVoltronPDU>();
                if (attribute != null)
                {
                    var enumType = (uint)attribute.Type;
                    string friendlyName = GetVoltronPacketTypeName((ushort)enumType) ?? "Unknown";
                    bool value = typeMap.TryAdd(enumType, type);
                    LogConsole($"Mapped {friendlyName}(0x{attribute.Type:X4}) to void {type.Name}()");                    
                }
                else 
                    TryMapSpecialVoltronPacket(type);
            }
            LogConsole($"Completed Successfully! {Assembly.FullName}", TSOLoggerServiceBase.LogSeverity.Warnings);
        }
        /// <summary>
        /// In derived classes, this method should be overridden to handle special Voltron packets that are not of base type <see cref="TSOVoltronPacket"/>.
        /// <para/>e.g. TSO Pre-Alpha DB Request Wrappers, etc.
        /// </summary>
        /// <param name="PacketType"></param>
        /// <param name="PDUData"></param>
        /// <returns></returns>
        protected abstract bool TryMapSpecialVoltronPacket(Type type);

        /// <summary>
        /// Uses the supplied <see cref="Stream"/> to read the stream contents and will return the first <see cref="TSOVoltronPacket"/> found.
        /// <para/> Note: This will start reading at the <see cref="Stream.Position"/> of the given stream, and will leave after the packet has been
        /// read. Please use <see cref="Stream.Seek(long, SeekOrigin)"/> to position the stream where the <see cref="TSOVoltronPacket"/> should be read
        /// from before calling.
        /// </summary>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public TSOVoltronPacket? CreatePacketObjectFromDataBuffer(Stream Stream)
        {
            TSOVoltronPacket? cTSOVoltronpacket = null;
            uint currentIndex = 0;
            long startPosition = Stream.Position;
                   
            int readBytes = TSOVoltronPacket.ReadVoltronHeader(Stream, out ushort VPacketType, out uint Size);
            currentIndex += Size;
            cTSOVoltronpacket = CreatePacketObjectByPacketType(VPacketType, Stream);
            Stream.Seek(startPosition, SeekOrigin.Begin);
            byte[] temporaryBuffer = new byte[Size];
            Stream.ReadExactly(temporaryBuffer, 0, (int)Math.Min(Stream.Length, (int)Size));
            if (cTSOVoltronpacket == null)            
                throw new NullReferenceException($"Reflected packet was null. T: {VPacketType} S: {Size}. Continuing...");            
            if (cTSOVoltronpacket is TSOBlankPDU)
            {
                Parent.Services.Get<TSOLoggerServiceBase>().OnVoltron_OnDiscoveryPacket(VPacketType, temporaryBuffer.Length);
                try
                {
                    LogDiscoveryPacketToDisk(VPacketType, temporaryBuffer);
                }
                catch (Exception ex)
                {
                    LogConsole($"ERROR: Could not save the discovery packet to file: {ex}",TSOLoggerServiceBase.LogSeverity.Errors);
                }
                return null;
            }
            cTSOVoltronpacket.ReflectFromBody(temporaryBuffer);
            cTSOVoltronpacket.EnsureNoErrors();// check for errors in PDU
            return cTSOVoltronpacket;
        }
        /// <summary>
        /// Uses the supplied <see cref="TSOTCPPacket"/> (Aries Packet) to read the body and find all enclosed <see cref="TSOVoltronPacket"/>s
        /// </summary>
        /// <param name="AriesPacket"></param>
        /// <param name="ProcessSplitBuffers"></param>
        /// <returns></returns>
        public IEnumerable<TSOVoltronPacket> CreatePacketObjectsFromAriesPacket(TSOTCPPacket AriesPacket, bool ProcessSplitBuffers = true)
        {
            List<TSOVoltronPacket> packets = new();
            TSOSplitBufferPDUCollection splitBufferCollection = new();

            AriesPacket.SetPosition(0);
            do
            {
                TSOVoltronPacket? cTSOVoltronpacket = default;
                cTSOVoltronpacket = CreatePacketObjectFromDataBuffer(AriesPacket.BodyStream);
                if (cTSOVoltronpacket != default)
                {
                    cTSOVoltronpacket.SenderQuazarConnectionID = AriesPacket.ConnectionID;
                    packets.Add(cTSOVoltronpacket);
                }
            }
            while (!AriesPacket.IsBodyEOF);
            return packets;
        }
        /// <summary>
        /// Instantiates the corresponding <see cref="TSOVoltronPacket"/> type with a given <see cref="TSO_PreAlpha_VoltronPacketTypes"/> value
        /// </summary>
        /// <param name="PacketType"></param>
        /// <param name="MakeBlankPacketOnFail"></param>
        /// <returns></returns>
        public TSOVoltronPacket? CreatePacketObjectByPacketType(uint PacketType, Stream PDUData, bool MakeBlankPacketOnFail = true)
        {
            TSOVoltronPacket? getReturnValue() => MakeBlankPacketOnFail ? new TSOBlankPDU(PacketType) : default;
            //Use the cTSOFactory Type map to reflect the packet type
            var specialPacket = TryGetSpecialVoltronPacket(PacketType, PDUData);
            if (specialPacket != null)
                return specialPacket;
            if (typeMap.TryGetValue(PacketType, out Type? type) && type?.FullName != null)
                return (type?.Assembly?.CreateInstance(type.FullName) as TSOVoltronPacket) ?? getReturnValue();
            return getReturnValue();
        }
        /// <summary>
        /// In derived classes, tries to retrieve a special Voltron packet type that is not in the main type map.
        /// </summary>
        /// <param name="PacketType"></param>
        /// <param name="PDUData"></param>
        /// <returns></returns>
        protected abstract TSOVoltronPacket? TryGetSpecialVoltronPacket(uint PacketType, Stream PDUData);

        /// <summary>
        /// Uses the supplied <paramref name="Packets"/> collection to merge them together into a <see cref="TSOVoltronPacket"/>
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public TSOVoltronPacket? CreatePacketObjectFromSplitBuffers(TSOSplitBufferPDUCollection Packets)
        {
            if (!Packets.Any()) throw new ArgumentOutOfRangeException(nameof(Packets));
            if (Packets.Count < 2) throw new ArgumentOutOfRangeException(nameof(Packets));

            int read = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (TSOSplitBufferPDUBase splitBuffer in Packets)
                {
                    read++;
                    stream.Write(splitBuffer.DataBuffer);
                    if (splitBuffer.EOF) break;
                }
                if (read != Packets.Count) throw new Exception("This may be totally safe. Supplied SplitBuffer Collection had " +
                    "more items than there were for this buffer.");
                stream.Seek(0, SeekOrigin.Begin);
                return CreatePacketObjectFromDataBuffer(stream);
            }
        }

        /// <summary>
        /// This will take one very large <see cref="TSOVoltronPacket"/> and create many smaller <see cref="TSOSplitBufferPDU"/>
        /// to send sequentially to the server over multiple Aries frames
        /// </summary>
        /// <param name="PDU"></param>
        /// <returns></returns>
        public TSOSplitBufferPDUCollection CreateSplitBufferPacketsFromPDU(TSOVoltronPacket PDU, uint SizeLimit = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize)
        {
            if (SizeLimit == 0)
                SizeLimit = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;
            if (PDU.BodyLength < SizeLimit)
                throw new InvalidOperationException("Splitting this PDU isn't necessary. It's too small.");

            using (MemoryStream ms = new(PDU.Body))
            {
                ms.Seek(0, SeekOrigin.Begin);

                long dataRemaining = ms.Length - ms.Position;

                TSOSplitBufferPDUCollection collection = new();
                while (ms.Position < ms.Length)
                {
                    byte[] buffer2 = new byte[Math.Min(SizeLimit, dataRemaining)];
                    ms.Read(buffer2, 0, buffer2.Length);
                    dataRemaining = ms.Length - ms.Position;
                    TSOSplitBufferPDUBase splitBuffer = CreateSplitBufferPDU(buffer2, dataRemaining == 0);
                    collection.Add(splitBuffer);
                }
                return collection;
            }
        }
        public abstract TSOSplitBufferPDUBase CreateSplitBufferPDU(byte[] DataBuffer, bool IsEOF);

        /// <summary>
        /// Writes a <see cref="TSOVoltronPacket"/> to the disk in the discoveries folder
        /// </summary>
        /// <param name="VoltronPacketType"></param>
        /// <param name="PacketData"></param>
        public bool LogDiscoveryPacketToDisk(ushort VoltronPacketType, byte[] PacketData)
        {
            string? displayName = GetVoltronPacketTypeName(VoltronPacketType);
            Directory.CreateDirectory(TSOVoltronConst.DiscoveriesDirectory);
            string fileName = Path.Combine(TSOVoltronConst.DiscoveriesDirectory, $"cTSOPDU [{displayName}].dat");
            bool existing = File.Exists(fileName);
            File.WriteAllBytes(fileName, PacketData);
            return !existing;
        }
        /// <summary>
        /// Gets the friendly name for the given <paramref name="PacketType"/>
        /// </summary>
        /// <param name="PacketType"></param>
        /// <returns></returns>
        public abstract string GetVoltronPacketTypeName(ushort VoltronPacketType);

        public abstract void Dispose();

        protected void LogConsole(string Message, TSOLoggerServiceBase.LogSeverity Severity = TSOLoggerServiceBase.LogSeverity.Message) => 
            Parent?.Logger?.LogConsole(new TSOLoggerServiceBase.ConsoleLogEntry(Severity, GetType().Name, Message));
    }
}

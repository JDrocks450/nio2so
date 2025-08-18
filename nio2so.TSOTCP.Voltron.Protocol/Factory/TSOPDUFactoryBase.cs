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

    public abstract class TSOPDUFactoryServiceBase : ITSOService
    {
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

        protected abstract void MapAssembly(Assembly assembly);
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
            {
                Parent.Services.Get<TSOLoggerServiceBase>().LogConsole(new(TSOLoggerServiceBase.LogSeverity.Errors,
                    GetType().Name, $"Reflected packet was null. T: {VPacketType} S: {Size}. Continuing..."));
                return null;
            }
            if (cTSOVoltronpacket is TSOBlankPDU)
            {
                Parent.Services.Get<TSOLoggerServiceBase>().OnVoltron_OnDiscoveryPacket(VPacketType, temporaryBuffer);
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
        public abstract TSOVoltronPacket? CreatePacketObjectByPacketType(uint PacketType, Stream PDUData, bool MakeBlankPacketOnFail = true);

        /// <summary>
        /// Uses the supplied <paramref name="Packets"/> collection to merge them together into a <see cref="TSOVoltronPacket"/>
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public abstract TSOVoltronPacket? CreatePacketObjectFromSplitBuffers(TSOSplitBufferPDUCollection Packets);        

        /// <summary>
        /// This will take one very large <see cref="TSOVoltronPacket"/> and create many smaller <see cref="TSOSplitBufferPDU"/>
        /// to send sequentially to the server over multiple Aries frames
        /// </summary>
        /// <param name="PDU"></param>
        /// <returns></returns>
        public abstract TSOSplitBufferPDUCollection CreateSplitBufferPacketsFromPDU(TSOVoltronPacket PDU, uint SizeLimit = TSOVoltronConst.SplitBufferPDU_DefaultChunkSize); 

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
    }
}

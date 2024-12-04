using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron.Collections;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.Factory
{
    /// <summary>
    /// This is an interface to use when reading/writing <see cref="TSOVoltronPacket"/>s
    /// </summary>
    internal static class TSOPDUFactory
    {
        private static Dictionary<TSO_PreAlpha_VoltronPacketTypes, Type> typeMap = new();
        private static Dictionary<TSO_PreAlpha_DBActionCLSIDs, Type> _dbtypeMap = new();

        static TSOPDUFactory()
        {
            foreach (var type in typeof(TSOPDUFactory).Assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<TSOVoltronPDU>();
                if (attribute != null)
                {
                    bool value = typeMap.TryAdd(attribute.Type, type);
                    QConsole.WriteLine("cTSOPDUFactory", $"Mapped {attribute.Type} to {type.Name}");
                    continue;
                }
                var dbattribute = type.GetCustomAttribute<TSOVoltronDBRequestWrapperPDU>();
                if (dbattribute != null)
                {
                    bool value = _dbtypeMap.TryAdd(dbattribute.Type, type);
                    QConsole.WriteLine("cTSOPDUFactory", $"Mapped *DB* {dbattribute.Type} to {type.Name}");
                }
            }
        }

        /// <summary>
        /// Uses the supplied <see cref="Stream"/> to read the stream contents and will return the first <see cref="TSOVoltronPacket"/> found.
        /// <para/> Note: This will start reading at the <see cref="Stream.Position"/> of the given stream, and will leave after the packet has been
        /// read. Please use <see cref="Stream.Seek(long, SeekOrigin)"/> to position the stream where the <see cref="TSOVoltronPacket"/> should be read
        /// from before calling.
        /// </summary>
        /// <param name="Stream"></param>
        /// <returns></returns>
        public static TSOVoltronPacket? CreatePacketObjectFromDataBuffer(Stream Stream)
        {
            TSOVoltronPacket? cTSOVoltronpacket = null;
            uint currentIndex = 0;
            TSOVoltronPacket.ReadVoltronHeader(Stream, out ushort VPacketType, out uint Size);
            currentIndex += Size;
            cTSOVoltronpacket = CreatePacketObjectByPacketType((TSO_PreAlpha_VoltronPacketTypes)VPacketType, Stream);
            byte[] temporaryBuffer = new byte[Size];
            Stream.ReadExactly(temporaryBuffer, 0, (int)Size);
            if (cTSOVoltronpacket == null)
            {
                TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Errors,
                    nameof(TSOPDUFactory), $"Reflected packet was null. T: {VPacketType} S: {Size}. Continuing..."));
                return null;
            }
            if (cTSOVoltronpacket is TSOBlankPDU)
            {
                TSOCityTelemetryServer.Global.OnVoltron_OnDiscoveryPacket(VPacketType, temporaryBuffer);
                return null;
            }
            cTSOVoltronpacket.ReflectFromBody(temporaryBuffer);
            return cTSOVoltronpacket;
        }

        /// <summary>
        /// Uses the supplied <see cref="TSOTCPPacket"/> (Aries Packet) to read the body and find all enclosed <see cref="TSOVoltronPacket"/>s
        /// </summary>
        /// <param name="AriesPacket"></param>
        /// <param name="ProcessSplitBuffers"></param>
        /// <returns></returns>
        public static IEnumerable<TSOVoltronPacket> CreatePacketObjectsFromAriesPacket(TSOTCPPacket AriesPacket, bool ProcessSplitBuffers = true)
        {
            List<TSOVoltronPacket> packets = new();
            TSOSplitBufferPDUCollection splitBufferCollection = new();

            AriesPacket.SetPosition(0);
            do
            {
                TSOVoltronPacket? cTSOVoltronpacket = default;
                try
                {
                    cTSOVoltronpacket = CreatePacketObjectFromDataBuffer(AriesPacket.BodyStream);
                }
                catch (Exception ex)
                {
                    QConsole.WriteLine("TSOVoltronPacket_Warnings", $"An error occured in the ParsePackets function. {ex.Message}");
                }
                if (cTSOVoltronpacket != default)
                {
                    cTSOVoltronpacket.EnsureNoErrors();// check for errors in PDU
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
        public static TSOVoltronPacket? CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes PacketType, Stream PDUData, bool MakeBlankPacketOnFail = true)
        {
            TSOVoltronPacket? getReturnValue() => MakeBlankPacketOnFail ? new TSOBlankPDU(PacketType) : default;
            
            switch (PacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU:
                    {
                        long streamPosition = PDUData.Position;
                        PDUData.Seek(streamPosition + TSODBRequestWrapper.DB_WRAPPER_ACTIONCLSID_INDEX, SeekOrigin.Begin);
                        byte[] clsIDbytes = new byte[sizeof(uint)];
                        PDUData.Read(clsIDbytes, 0, sizeof(uint));
                        uint clsIDint32 = EndianBitConverter.Big.ToUInt32(clsIDbytes,0);
                        PDUData.Seek(streamPosition, SeekOrigin.Begin);

                        TSO_PreAlpha_DBActionCLSIDs clsID = (TSO_PreAlpha_DBActionCLSIDs)clsIDint32;
                        //Use reflection to make corresponding type of DBWrapper packet format
                        if (_dbtypeMap.TryGetValue(clsID, out var dbtype))
                        {
                            var retValue = (TSODBRequestWrapper)dbtype?.Assembly?.CreateInstance(dbtype.FullName);
                            if (retValue != null) return retValue;
                            //Let case fall through to default Type map implementation below
                        }
                    }
                    break;
                case TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU:
                    return new TSOHostOnlinePDU();
                case TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU:
                    return new TSOClientOnlinePDU();                                        
            }
            //Use the cTSOFactory Type map to reflect the packet type
            if (typeMap.TryGetValue(PacketType, out var type))
                return (TSOVoltronPacket)type?.Assembly?.CreateInstance(type.FullName) ?? getReturnValue();
            return getReturnValue();
        }
        /// <summary>
        /// Uses the supplied <paramref name="Packets"/> collection to merge them together into a <see cref="TSOVoltronPacket"/>
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public static TSOVoltronPacket? CreatePacketObjectFromSplitBuffers(TSOSplitBufferPDUCollection Packets)
        {
            if (!Packets.Any()) throw new ArgumentOutOfRangeException(nameof(Packets));
            if (Packets.Count < 2) throw new ArgumentOutOfRangeException(nameof(Packets));

            int read = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (var splitBuffer in Packets)
                {
                    read++;
                    stream.Write(splitBuffer.DataBuffer);
                    if (!splitBuffer.HasDataRemaining) break;
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
        public static TSOSplitBufferPDUCollection CreateSplitBufferPacketsFromPDU(TSOVoltronPacket PDU,
            uint SizeLimit = TSOSplitBufferPDU.STANDARD_CHUNK_SIZE)
        {
            if (PDU.BodyLength < SizeLimit)
                throw new InvalidOperationException("Splitting this PDU isn't necessary. It's too small.");

            using (MemoryStream ms = new(PDU.Body))
            {
                ms.Seek(0, SeekOrigin.Begin);

                long dataRemaining = ms.Length - ms.Position;

                TSOSplitBufferPDUCollection collection = new();
                while (ms.Position < ms.Length)
                {
                    byte[] buffer2 = new byte[Math.Min(SizeLimit,dataRemaining)];
                    ms.Read(buffer2, 0, buffer2.Length);
                    dataRemaining = ms.Length - ms.Position;
                    TSOSplitBufferPDU splitBuffer = new(buffer2, dataRemaining > 0);
                    collection.Add(splitBuffer);
                }
                return collection;
            }
        }

        /// <summary>
        /// Writes a <see cref="TSOVoltronPacket"/> to the disk in the discoveries folder
        /// </summary>
        /// <param name="VoltronPacketType"></param>
        /// <param name="PacketData"></param>
        public static bool LogDiscoveryPacketToDisk(ushort VoltronPacketType, byte[] PacketData)
        {
            string? displayName = Enum.GetName((TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType) ??
                            "0x" + VoltronPacketType.ToString("X4");
            Directory.CreateDirectory(TSOVoltronConst.DiscoveriesDirectory);
            string fileName = Path.Combine(TSOVoltronConst.DiscoveriesDirectory,$"cTSOPDU [{displayName}].dat");
            bool existing = File.Exists(fileName);
            File.WriteAllBytes(fileName, PacketData);
            return !existing;
        }
    }
}

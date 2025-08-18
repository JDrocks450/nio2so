using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Collections;
using nio2so.Voltron.Core.TSO.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using System.Reflection;

namespace nio2so.Voltron.PreAlpha.Protocol.Services
{    
    /// <summary>
    /// This is an interface to use when reading/writing <see cref="TSOVoltronPacket"/>s
    /// </summary>
    public class TSOPreAlphaPDUFactory : TSOPDUFactoryServiceBase
    {
        private Dictionary<TSO_PreAlpha_VoltronPacketTypes, Type> typeMap = new();
        private Dictionary<TSO_PreAlpha_DBActionCLSIDs, Type> _dbtypeMap = new();
        private Dictionary<TSO_PreAlpha_MasterConstantsTable, Type> _broadcastDatablobTypeMap = new();

        /// <summary>
        /// Maps the given assemblies to the factory.
        /// </summary>
        /// <param name="Assemblies"></param>
        public TSOPreAlphaPDUFactory(params Assembly[] Assemblies) : base(Assemblies) { }
        /// <summary>
        /// Maps the following assemblies: <c>PreAlpha.Protocol, TSO.Core</c>
        /// </summary>
        public TSOPreAlphaPDUFactory() : this(typeof(TSOPreAlphaPDUFactory).Assembly, typeof(TSOVoltronPacket).Assembly) { }

        protected override void MapAssembly(Assembly Assembly)
        {
            Parent.Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Warnings,
                       GetType().Name, $"\nBeginning to map: {Assembly.FullName}\n"));
            foreach (var type in Assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<TSOVoltronPDU>();
                if (attribute != null)
                {
                    var enumType = (TSO_PreAlpha_VoltronPacketTypes)(uint)attribute.Type;
                    string friendlyName = Enum.GetName(enumType) ?? "Unknown";                    
                    bool value = typeMap.TryAdd(enumType, type);
                    Parent.Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message,
                        GetType().Name, $"Mapped {enumType}(0x{attribute.Type:X4}) to void {type.Name}()"));
                    continue;
                }
                var dbattribute = type.GetCustomAttribute<TSOVoltronDBRequestWrapperPDU>();
                if (dbattribute != null)
                {
                    var enumType = (TSO_PreAlpha_DBActionCLSIDs)(uint)dbattribute.Type;
                    string friendlyName = Enum.GetName(enumType) ?? "Unknown";
                    bool value = _dbtypeMap.TryAdd(enumType, type);
                    Parent.Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message,
                       GetType().Name, $"Mapped *DB* {enumType}(0x{dbattribute.Type:X4}) to void {type.Name}()"));
                    continue;
                }
            }
            Parent.Logger.LogConsole(new(TSOLoggerServiceBase.LogSeverity.Warnings,
                       GetType().Name, $"Completed Successfully! {Assembly.FullName}"));
        }
        public override TSOSplitBufferPDUCollection CreateSplitBufferPacketsFromPDU(TSOVoltronPacket PDU, uint SizeLimit = 500)
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
                    TSOSplitBufferPDU splitBuffer = new(buffer2, dataRemaining == 0);
                    collection.Add(splitBuffer);
                }
                return collection;
            }
        }
        public override TSOVoltronPacket? CreatePacketObjectByPacketType(uint PacketType, Stream PDUData, bool MakeBlankPacketOnFail = true)
        {
            TSOVoltronPacket? getReturnValue() => MakeBlankPacketOnFail ? new TSOBlankPDU(PacketType) : default;

            TSO_PreAlpha_VoltronPacketTypes KnownPacketType = (TSO_PreAlpha_VoltronPacketTypes)PacketType;
            switch (KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU:
                    {
                        PDUData.Position += 6; // advance past voltron header
                        TSO_PreAlpha_DBActionCLSIDs clsID = TSODBRequestWrapper.ReadDBPDUHeader(PDUData).ActionType;
                        //Use reflection to make corresponding type of DBWrapper packet format
                        if (_dbtypeMap.TryGetValue(clsID, out var dbtype))
                        {
                            var retValue = dbtype?.Assembly?.CreateInstance(dbtype.FullName) as TSODBRequestWrapper;
                            if (retValue != null) return retValue;
                            throw new InvalidDataException($"Your type: {dbtype.Name} is not a {nameof(TSODBRequestWrapper)}! " +
                                $"You must fix this and recompile.");
                            //Let case fall through to default Type map implementation below
                        }
                    }
                    break;
            }
            //Use the cTSOFactory Type map to reflect the packet type
            if (typeMap.TryGetValue(KnownPacketType, out var type))
                return (TSOVoltronPacket)type?.Assembly?.CreateInstance(type.FullName) ?? getReturnValue();
            return getReturnValue();
        }

        public override void Dispose()
        {
            
        }

        public override string GetVoltronPacketTypeName(ushort VoltronPacketType) => Enum.GetName((TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType) ?? "0x" + VoltronPacketType.ToString("X4");

        public override TSOVoltronPacket? CreatePacketObjectFromSplitBuffers(TSOSplitBufferPDUCollection Packets)
        {
            if (!Packets.Any()) throw new ArgumentOutOfRangeException(nameof(Packets));
            if (Packets.Count < 2) throw new ArgumentOutOfRangeException(nameof(Packets));

            int read = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (TSOSplitBufferPDU splitBuffer in Packets)
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
    }
}

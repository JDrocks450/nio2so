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
    /// For use with The Sims Online Pre-Alpha Voltron Server. <para/>
    /// This is an interface to use when reading/writing <see cref="TSOVoltronPacket"/>, <see cref="TSODBRequestWrapper"/>, and TSOBroadcastPDU.
    /// </summary>
    public class TSOPreAlphaPDUFactory : TSOPDUFactoryServiceBase
    {        
        private Dictionary<TSO_PreAlpha_DBActionCLSIDs, Type> _dbtypeMap = new();

        /// <summary>
        /// Maps the given assemblies to the factory.
        /// </summary>
        /// <param name="Assemblies"></param>
        public TSOPreAlphaPDUFactory(params Assembly[] Assemblies) : base(Assemblies) { }
        /// <summary>
        /// Maps the following assemblies: <c>PreAlpha.Protocol, TSO.Core</c>
        /// </summary>
        public TSOPreAlphaPDUFactory() : this(typeof(TSOPreAlphaPDUFactory).Assembly, typeof(TSOVoltronPacket).Assembly) { }

        protected override bool TryMapSpecialVoltronPacket(Type type)
        {
            var dbattribute = type.GetCustomAttribute<TSOVoltronDBRequestWrapperPDU>();
            if (dbattribute != null)
            {
                var enumType = (TSO_PreAlpha_DBActionCLSIDs)(uint)dbattribute.Type;
                string friendlyName = Enum.GetName(enumType) ?? "Unknown";
                bool value = _dbtypeMap.TryAdd(enumType, type);
                LogConsole($"Mapped *DB* {enumType}(0x{dbattribute.Type:X4}) to void {type.Name}()");
                return true;
            }
            return false;
        }

        protected override TSOVoltronPacket? TryGetSpecialVoltronPacket(uint PacketType, Stream PDUData)
        {           
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
                            if (retValue != null) 
                                return retValue;
                            throw new InvalidDataException($"Your type: {dbtype.Name} is not a {nameof(TSODBRequestWrapper)}! " +
                                $"You must fix this and recompile.");
                            //Let case fall through to default Type map implementation below
                        }
                    }
                    break;
            }
            return null;
        }        

        public override string GetVoltronPacketTypeName(ushort VoltronPacketType) => Enum.GetName((TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType) ?? "0x" + VoltronPacketType.ToString("X4");
        public override TSOSplitBufferPDUBase CreateSplitBufferPDU(byte[] DataBuffer, bool IsEOF) => new TSOPreAlphaSplitBufferPDU(DataBuffer, IsEOF);
        public override void Dispose()
        {
            
        }
    }
}

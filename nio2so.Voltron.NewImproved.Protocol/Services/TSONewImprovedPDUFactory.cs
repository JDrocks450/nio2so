using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using System.Reflection;

namespace nio2so.Voltron.NewImproved.Protocol.Services
{   
    /// <summary>
    /// For use with The Sims Online New and Improved Voltron Server. <para/>
    /// This is an interface to use when reading/writing .. i dont know yet!
    /// </summary>
    public class TSONewImprovedPDUFactory : TSOPDUFactoryServiceBase
    {
        private Dictionary<TSO_NewImproved_VoltronPacketTypes_MsgCLSIDs, Type> _dataServicetypeMap = new();

        /// <summary>
        /// Maps the given assemblies to the factory.
        /// </summary>
        /// <param name="Assemblies"></param>
        public TSONewImprovedPDUFactory(params Assembly[] Assemblies) : base(Assemblies) { }
        /// <summary>
        /// Maps the following assemblies: <c>PreAlpha.Protocol, TSO.Core</c>
        /// </summary>
        public TSONewImprovedPDUFactory() : this(typeof(TSONewImprovedPDUFactory).Assembly, typeof(TSOVoltronPacket).Assembly) { }

        protected override bool TryMapSpecialVoltronPacket(Type type)
        {
            return false;
        }

        protected override TSOVoltronPacket? TryGetSpecialVoltronPacket(uint PacketType, Stream PDUData)
        {
            //create a new data service wrapper
            if (PacketType == (ushort)TSO_NewImproved_VoltronPacketTypes.DataServiceWrapperPDU)            
                return new TSODataServiceWrapperPDU();            
            return null;
        }

        public override string GetVoltronPacketTypeName(ushort VoltronPacketType) => Enum.GetName((TSO_NewImproved_VoltronPacketTypes)VoltronPacketType) ?? "0x" + VoltronPacketType.ToString("X4");
        public override TSOSplitBufferPDUBase CreateSplitBufferPDU(byte[] DataBuffer, bool IsEOF) => new TSONewImprovedSplitBufferPDU(DataBuffer, IsEOF);
        public override void Dispose()
        {

        }
    }
}

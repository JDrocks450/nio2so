using nio2so.Voltron.Core.Factory;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.PDU;
using nio2so.Voltron.PlayTest.Protocol.PDU;
using System.Reflection;

namespace nio2so.Voltron.PlayTest.Protocol.Services
{
    /// <summary>
    /// For use with The Sims Online Play-Test Voltron Server. <para/>
    /// This is an interface to use when reading/writing .. i dont know yet!
    /// </summary>
    public class TSOPlayTestPDUFactory : TSOPDUFactoryServiceBase
    {
        //private Dictionary<TSO_PreAlpha_DBActionCLSIDs, Type> _dbtypeMap = new();

        /// <summary>
        /// Maps the given assemblies to the factory.
        /// </summary>
        /// <param name="Assemblies"></param>
        public TSOPlayTestPDUFactory(params Assembly[] Assemblies) : base(Assemblies) { }
        /// <summary>
        /// Maps the following assemblies: <c>PreAlpha.Protocol, TSO.Core</c>
        /// </summary>
        public TSOPlayTestPDUFactory() : this(typeof(TSOPlayTestPDUFactory).Assembly, typeof(TSOVoltronPacket).Assembly) { }

        protected override bool TryMapSpecialVoltronPacket(Type type)
        {
            return false;
        }

        protected override TSOVoltronPacket? TryGetSpecialVoltronPacket(uint PacketType, Stream PDUData)
        {

            return null;
        }

        public override string GetVoltronPacketTypeName(ushort VoltronPacketType) => Enum.GetName((TSO_PlayTest_VoltronPacketTypes)VoltronPacketType) ?? "0x" + VoltronPacketType.ToString("X4");
        public override TSOSplitBufferPDUBase CreateSplitBufferPDU(byte[] DataBuffer, bool IsEOF) => new TSOPlayTestSplitBufferPDU(DataBuffer, IsEOF);
        public override void Dispose()
        {

        }
    }
}

using nio2so.Data.Common;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using System.Reflection;

namespace nio2so.Voltron.PlayTest.Protocol.Regulator
{
    /// <summary>
    /// For use with The Sims Online Play-test Voltron Server.<para/>
    /// <inheritdoc cref="TSOProtocolBase"/>
    /// </summary>
    public abstract class TSOProtocol : TSOProtocolBase
    {
        /// <summary>
        /// <inheritdoc cref="TSOProtocolBase.TSOProtocolBase(TSOProtocolMatchingOption[])"/>
        /// </summary>
        protected TSOProtocol() : base(
                TSOProtocolMatchingOption.CreateVoltron<TSO_PlayTest_VoltronPacketTypes>()
            )
        { }

        protected override bool TryHandleSpecialVoltronPDU(TSOVoltronPacket PDU, ref TSOProtocolRegulatorResponse Response)
        {
            // Special VoltronPDU handling for TSO_PlayTest_Packets

            return false;
        }
    }
}

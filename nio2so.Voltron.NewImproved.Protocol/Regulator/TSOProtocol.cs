using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator
{
    /// <summary>
    /// For use with The Sims Online Play-test Voltron Server.<para/>
    /// <inheritdoc cref="TSOProtocolBase"/>
    /// </summary>
    public abstract class TSOProtocol : TSOProtocolBase
    {
        /// <summary>
        /// <inheritdoc cref="TSOProtocolBase(TSOProtocolMatchingOption[])"/>
        /// </summary>
        protected TSOProtocol() : base(
                TSOProtocolMatchingOption.CreateVoltron<TSO_NewImproved_VoltronPacketTypes>()
            )
        { }

        protected override bool TryHandleSpecialVoltronPDU(TSOVoltronPacket PDU, ref TSOProtocolRegulatorResponse Response)
        {
            return false;
        }
    }
}

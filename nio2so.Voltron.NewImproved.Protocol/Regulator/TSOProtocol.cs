using nio2so.DataService.Common.Tokens;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.Core.TSO.Struct;

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

        /// <summary>
        /// NOT FULLY IMPLEMENTED YET
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public override TSOAriesIDStruct GetVoltronIDStruct(AvatarIDToken AvatarID) => new TSOAriesIDStruct(AvatarID, "default");
        public override TSOPlayerInfoStruct GetPlayerInfoStruct(AvatarIDToken AvatarID)
        {
            TSOAriesIDStruct VoltronID = GetVoltronIDStruct(AvatarID);
            return new TSOPlayerInfoStruct(VoltronID, 0x41, true);
        }

    }
}

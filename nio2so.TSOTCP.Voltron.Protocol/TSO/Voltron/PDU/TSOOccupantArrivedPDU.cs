using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// <code>Unverified!</code>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_ARRIVED_PDU)]
    public class TSOOccupantArrivedPDU : TSOVoltronPacket
    {
        public TSOOccupantArrivedPDU() : base() { }
        public TSOOccupantArrivedPDU(TSOAriesIDStruct PlayerID)
        {
            PlayerInfo = new(PlayerID);
            MakeBodyFromProperties();
        }
        /// <summary>
        /// The <see cref="TSOPlayerInfoStruct"/> of the newly joined player
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_ARRIVED_PDU;
    }
}

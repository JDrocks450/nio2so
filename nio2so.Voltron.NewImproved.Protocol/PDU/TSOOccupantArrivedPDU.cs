using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    /// <summary>
    /// Sent to a Host of a room when a new TSOClient joins their session
    /// </summary>
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.OccupantArrivedPDU)]
    public class TSOOccupantArrivedPDU : TSOVoltronPacket
    {
        public TSOOccupantArrivedPDU() : base() { }
        public TSOOccupantArrivedPDU(TSOPlayerInfoStruct PlayerInfo) : this()
        {
            this.PlayerInfo = PlayerInfo;
            MakeBodyFromProperties();
        }
        /// <summary>
        /// The <see cref="TSOPlayerInfoStruct"/> of the newly joined player
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.OccupantArrivedPDU;
    }
}

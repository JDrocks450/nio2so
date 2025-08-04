using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent to a Host of a room when a TSOClient is leaving the session
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_DEPARTED_PDU)]
    public class TSOOccupantDepartedPDU : TSOVoltronPacket
    {
        public TSOOccupantDepartedPDU() : base() { }
        public TSOOccupantDepartedPDU(TSOPlayerInfoStruct PlayerInfo) : this()
        {
            this.PlayerInfo = PlayerInfo;
            MakeBodyFromProperties();
        }
        /// <summary>
        /// The <see cref="TSOPlayerInfoStruct"/> of the now departed player
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_DEPARTED_PDU;
    }
}

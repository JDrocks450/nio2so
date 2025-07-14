using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Updates the <c>m_PlayerInfo</c> this client is acting as
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU)]
    public class TSOUpdatePlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU;
        /// <summary>
        /// The new <see cref="TSOPlayerInfoStruct"/> (<c>m_playerInfo</c>) to update the value to
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; }
        /// <summary>
        /// Creates a new <see cref="TSOUpdatePlayerPDU"/> and creates a new <see cref="TSOPlayerInfoStruct"/> using <paramref name="PlayerID"/>
        /// </summary>
        /// <param name="PlayerID"></param>
        public TSOUpdatePlayerPDU(TSOAriesIDStruct PlayerID)
        {
            PlayerInfo = new(PlayerID);
            MakeBodyFromProperties();
        }
        /// <summary>
        /// Creates a new <see cref="TSOUpdatePlayerPDU"/> with the given <see cref="TSOPlayerInfoStruct"/>
        /// </summary>
        /// <param name="PlayerID"></param>
        public TSOUpdatePlayerPDU(TSOPlayerInfoStruct PlayerInfo) : this()
        {
            this.PlayerInfo = PlayerInfo;
            MakeBodyFromProperties();
        }

        public TSOUpdatePlayerPDU() : base() { MakeBodyFromProperties(); }
    }
}

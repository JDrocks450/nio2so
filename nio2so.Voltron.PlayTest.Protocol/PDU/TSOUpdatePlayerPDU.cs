using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;

namespace nio2so.Voltron.PlayTest.Protocol.PDU
{
    /// <summary>
    /// Updates the <c>m_PlayerInfo</c> this client is acting as
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PlayTest_VoltronPacketTypes.UpdatePlayerPDU)]
    public class TSOUpdatePlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PlayTest_VoltronPacketTypes.UpdatePlayerPDU;
        /// <summary>
        /// The new <see cref="TSOPlayerInfoStruct"/> (<c>m_playerInfo</c>) to update the value to
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; }
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

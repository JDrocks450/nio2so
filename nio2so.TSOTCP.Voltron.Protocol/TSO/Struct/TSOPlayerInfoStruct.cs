namespace nio2so.Voltron.Core.TSO.Struct
{
    /// <summary>
    /// <c>Maps to m_pPlayerInfo</c> Which contains information on a player for the <see cref="TSOFindPlayerResponsePDU"/>
    /// </summary>
    public record TSOPlayerInfoStruct
    {
        public TSOPlayerInfoStruct()
        {
        }
        /// <summary>
        /// Creates a <see cref="TSOPlayerInfoStruct"/>
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="badge"></param>
        /// <param name="isAlertable"></param>
        public TSOPlayerInfoStruct(TSOAriesIDStruct playerID, byte badge = 0x0, bool isAlertable = true) : this()
        {
            PlayerID = playerID;
            Badge = badge;
            IsAlertable = isAlertable;
        }
        public TSOPlayerInfoStruct(TSOPlayerInfoStruct other) : base()
        {
            PlayerID = other.PlayerID;
            Badge = other.Badge;
            IsAlertable = other.IsAlertable;
        }

        /// <summary>
        /// <c>Maps to m_PlayerID</c>
        /// </summary>
        public TSOAriesIDStruct PlayerID { get; set; }
        /// <summary>
        /// <c>Maps to s_m_badge</c> The badge shown on the SimPage
        /// </summary>
        public byte Badge { get; set; }
        /// <summary>
        /// <c>Maps to s_m_isAlertable</c> Can this player get Instant Messages?
        /// </summary>
        public bool IsAlertable { get; set; }
    }
}

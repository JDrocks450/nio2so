namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
#if TSONANDI
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.ServerByePDU)]
    public class TSOServerBye : TSOVoltronPacket
    {
        public TSOServerBye() : base()
        {
        }

        public TSOServerBye(uint reasonCode, string reasonText = "") : this()
        {
            ReasonCode = reasonCode;
            ReasonText = reasonText;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// m_ReasonCode
        /// </summary>
        public uint ReasonCode { get; set; }
        /// <summary>
        /// m_ReasonText
        /// </summary>
        public string ReasonText { get; set; } = "";

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.ServerByePDU;
    }
#endif
}

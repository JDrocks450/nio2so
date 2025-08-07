namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    /// <summary>
    /// Ignored by the client, likely server only command to tell the server to eject someone
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.EJECT_VISITOR_PDU)]
    public class TSOEjectVisitorPDU : TSOVoltronPacket
    {
        public uint AvatarID { get; set; }

        public TSOEjectVisitorPDU()
        {
        }

        public TSOEjectVisitorPDU(uint avatarID)
        {
            AvatarID = avatarID;
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.EJECT_VISITOR_PDU;
    }
}

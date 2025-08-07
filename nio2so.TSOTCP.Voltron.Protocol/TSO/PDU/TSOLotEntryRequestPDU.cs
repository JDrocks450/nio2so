namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    /// <summary>
    /// Sent when the remote connection clicks "Join House" in the interface
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
    public class TSOLotEntryRequestPDU : TSOVoltronPacket
    {
        public ushort Lot_X { get; set; }
        public ushort Lot_Y { get; set; }
        public uint HouseID { get; set; }

        public TSOLotEntryRequestPDU() : base()
        {

        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU;
    }
}

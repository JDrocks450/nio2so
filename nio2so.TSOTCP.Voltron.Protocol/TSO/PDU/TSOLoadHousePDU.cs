namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    /// <summary>
    /// A PDU sent from the Client when asking for a House to load.
    /// <para>Found to be called after SetHouseBlobByID was sent from the server</para>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
    public class TSOLoadHousePDU : TSOVoltronPacket
    {
        /// <summary>
        /// Optional parameter, could be sending avatar id
        /// </summary>
        public uint HouseID { get; set; }

        public TSOLoadHousePDU() : base() { }
        public TSOLoadHousePDU(uint houseID) : base()
        {
            HouseID = houseID;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU;
    }
}

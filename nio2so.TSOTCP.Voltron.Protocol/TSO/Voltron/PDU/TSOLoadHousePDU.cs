namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// A PDU sent from the Client when asking for a House to load.
    /// <para>Found to be called after SetHouseBlobByID was sent from the server</para>
    /// <code>Unverified!</code>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
    public class TSOLoadHousePDU : TSOVoltronPacket
    {
        /// <summary>
        /// Can be absent from packet body in some circumstances
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

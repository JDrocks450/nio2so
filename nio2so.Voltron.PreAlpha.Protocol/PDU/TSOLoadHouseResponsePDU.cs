using nio2so.Voltron.Core.TSO;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// This PDU is stubbed out by the Client and will raise an exception without reading the response body
    /// <c>WARNING:</c>
    /// <para>The TSOClient uses this PDU in a very interesting way. Therefore, in current spec, this packet type is never actually 
    /// sent to the Client, and <see cref="TSO_PreAlpha_VoltronPacketTypes.HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU"/> will be sent 
    /// instead.</para>
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
    public class TSOLoadHouseResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU;
        /// <summary>
        /// One UInt32 with an ID to load.
        /// </summary>
        public uint HouseID { get; set; }

        public TSOLoadHouseResponsePDU() : base() { MakeBodyFromProperties(); }

        public TSOLoadHouseResponsePDU(uint houseID) : base()
        {
            HouseID = houseID;
            MakeBodyFromProperties();
        }
    }
}

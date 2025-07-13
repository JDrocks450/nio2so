namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// The <see cref="TSOHostOnlinePDU"/> sends to the Client that this server is a Voltron server.
    /// <para>Encompasses a HostVersion and PacketSize</para>
    /// <code>Unverified!</code>
    /// </summary>
    public class TSOHostOnlinePDU : TSOVoltronPacket
    {
        //CONST/TUNING
        const ushort PACKET_SIZE_LIMIT = 0x0;// TSO.TSOCityServer.DefaultSendAmt;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU;

        public ushort SizeLimit { get; }
        public uint Arg3 { get; }
        public TSOHostOnlinePDU(ushort packetSize = PACKET_SIZE_LIMIT, uint arg3 = 0x7FFF7FFF)
        {
            SizeLimit = packetSize;
            Arg3 = arg3;
            MakeBodyFromProperties();
        }
    }
}

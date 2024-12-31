namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{

    internal class TSOUpdatePlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_PLAYER_PDU;
        public string AriesID { get; set; }
        public string MasterID { get; set; }
        public byte Arg2 { get; set; } = 0x41;
        public byte Arg3 { get; set; } = 0x01;

        public TSOUpdatePlayerPDU(uint AvatarID, string AvatarName)
        {
            AriesID = $"A {AvatarID}";
            MasterID = AvatarName;
            MakeBodyFromProperties();
        }
    }

#if false
    internal class TSOHostOnlinePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => 0x1D;
        public ushort NumberOfWords => (ushort)HostReservedWords.Length;
        public string[] HostReservedWords { get; }
        public ushort HostVersion { get; }
        public ushort SendLimit { get; }

        protected TSOHostOnlinePDU(TSOTCPPacket Packet) : base(Packet)
        {

        }

        public TSOHostOnlinePDU(ushort HostVersion, ushort SendLimit = TSOCityServer.TSO_Aries_SendRecvLimit, params string[] HostReservedWords)
        {
            this.HostVersion = HostVersion;
            this.SendLimit = SendLimit;
            this.HostReservedWords = HostReservedWords;
            ReflectProperties2Buffer();
        }

        public override T ParseFromAriesPacket<T>(TSOTCPPacket AriesPacket)
        {
            throw new NotImplementedException();
        }
    }
#endif
}

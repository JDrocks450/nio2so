using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
    public class TSOChatMessagePDU : TSOVoltronPacket
    {
        public byte Arg1 { get; set; }
        /// <summary>
        /// The person who sent the Chat Message
        /// </summary>
        public TSOAriesIDStruct Person { get; set; }
        public ushort Arg2 { get; set; }
        public string Message { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU;

        public TSOChatMessagePDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOChatMessagePDU(TSOAriesIDStruct User, string ChatMessage) : this()
        {
            Arg1 = 0x01;
            Person = User;
            Arg2 = 0x00;
            Message = ChatMessage;

            MakeBodyFromProperties();
        }
    }
}

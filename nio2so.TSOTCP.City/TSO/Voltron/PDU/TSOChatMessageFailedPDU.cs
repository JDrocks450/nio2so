﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_FAILED_PDU)]
    internal class TSOChatMessageFailedPDU : TSOVoltronPacket
    {
        public byte Arg1 { get; set; }
        public string Str1 { get; set; }
        public string Str2 { get; set; }
        public ushort Arg2 { get; set; }
        public string Message {  get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU;

        public TSOChatMessageFailedPDU() : base() {
            MakeBodyFromProperties();
        }

        public TSOChatMessageFailedPDU(string ChatMessage) : this()
        {
            Arg1 = 0x01;
            Str1 = "A 1337";
            Str2 = "bsiquikc";
            Arg2 = 0x00;
            Message = ChatMessage;

            MakeBodyFromProperties();
        }
    }
}

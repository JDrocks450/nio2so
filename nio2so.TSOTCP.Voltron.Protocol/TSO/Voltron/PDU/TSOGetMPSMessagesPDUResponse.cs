﻿using nio2so.Data.Common.Testing;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_RESPONSE_PDU)]
    public class TSOGetMPSMessagesPDUResponse : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_RESPONSE_PDU;
        public uint Arg1 { get; set; } = 0x01;
        public uint AvatarID { get; set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public string Message { get; set; }

        public TSOGetMPSMessagesPDUResponse() : base()
        {
            AvatarID = TestingConstraints.MyFriendAvatarID;
            Title = TestingConstraints.MyFriendAvatarName;
            Caption = "Hello world";
            Message = "I am bisquick, ruler of all";

            MakeBodyFromProperties();
        }
    }
}

﻿using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.JOIN_ROOM_FAILED_PDU)]
    public class TSOJoinRoomFailedPDU : TSOVoltronPacket
    {
        public uint StatusCode { get; set; } = 0;
        public string ReasonText { get; set; } = "";
        public TSORoomIDStruct RoomID { get; set; } = new();

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.JOIN_ROOM_FAILED_PDU;

        public TSOJoinRoomFailedPDU() : base()
        {

        }

        public TSOJoinRoomFailedPDU(uint statusCode, string reasonText, TSORoomIDStruct roomID) : this()
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            RoomID = roomID;
            MakeBodyFromProperties();
        }
    }
}
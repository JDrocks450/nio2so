using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_ARRIVED_PDU)]
    public class TSOOccupantArrivedPDU : TSOVoltronPacket
    {
        public TSOOccupantArrivedPDU() : base() { }
        public TSOOccupantArrivedPDU(uint avatarID, string avatarName)
        {            
            AvatarID = TSOAriesIDStruct.FormatIDString(avatarID);
            AvatarName = avatarName;
        }

        [TSOVoltronString] public string AvatarID { get; set; }
        [TSOVoltronString] public string AvatarName { get; set; }
        public byte Arg1 { get; set; } = 0x41;
        public byte Arg2 { get; set; } = 0x01;
        public byte[] Garbage { get; set; } = new byte[48];

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_ARRIVED_PDU;
    }
}

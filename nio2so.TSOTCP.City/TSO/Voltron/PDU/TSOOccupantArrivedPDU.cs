using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_ARRIVED_PDU)]
    internal class TSOOccupantArrivedPDU : TSOVoltronPacket
    {
        public TSOOccupantArrivedPDU(string avatarID, string avatarName)
        {
            AvatarID = avatarID;
            AvatarName = avatarName;
        }

        [TSOVoltronString] public string AvatarID { get; set; }
        [TSOVoltronString] public string AvatarName { get; set; }
        public byte Arg1 { get; set; } = 0x41;
        public byte Arg2 { get; set; } = 0x01;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.OCCUPANT_ARRIVED_PDU;
    }
}

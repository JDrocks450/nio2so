using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Confirms a <see cref="TSOEjectVisitorPDU"/> but the client does not process this PDU
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.EJECT_VISITOR_RESPONSE_PDU)]
    public class TSOEjectVisitorResponse : TSOVoltronPacket
    {
        public TSOEjectVisitorResponse()
        {
        }

        public TSOEjectVisitorResponse(uint avatarID)
        {
            AvatarID = avatarID;
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.EJECT_VISITOR_RESPONSE_PDU;
        public uint AvatarID { get; set; }

    }
}

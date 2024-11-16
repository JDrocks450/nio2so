using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU)]
    internal class TSOFindPlayerResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_RESPONSE_PDU;

        public uint StatusCode { get; set; } = 0x0;
        [TSOVoltronString] public string ReasonString { get; set; } = "";

        public TSOFindPlayerResponsePDU() : base() {
            MakeBodyFromProperties();
        }    
    }
}

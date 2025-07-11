using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_RESPONSE_PDU)]
    public class TSOFlashMessageResponsePDU : TSOVoltronPacket
    {
        public uint ReasonCode { get; set; } = 0x01;
        public string ReasonText { get; set; } = "OK";

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_PDU;
    }
}

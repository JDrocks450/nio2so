using nio2so.Voltron.Core.TSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PlayTest.Protocol.PDU
{
#if TSONANDI
    [TSOVoltronPDU((uint)TSO_PlayTest_VoltronPacketTypes.ServerByePDU)]
    public class TSOServerBye : TSOVoltronPacket
    {
        public TSOServerBye() : base()
        {
        }

        public TSOServerBye(uint reasonCode, string reasonText = "") : this()
        {
            ReasonCode = reasonCode;
            ReasonText = reasonText;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// m_ReasonCode
        /// </summary>
        public uint ReasonCode { get; set; }
        /// <summary>
        /// m_ReasonText
        /// </summary>
        public string ReasonText { get; set; } = "";

        public override ushort VoltronPacketType => (ushort)TSO_PlayTest_VoltronPacketTypes.ServerByePDU;
    }
#endif
}

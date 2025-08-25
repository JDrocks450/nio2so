using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PlayTest.Protocol.PDU.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PlayTest.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PlayTest_VoltronPacketTypes.DBRequestWrapperPDU)]
    public class TSODBRequestWrapperPDU : TSOVoltronPacket
    {
        public TSODBRequestWrapperPDU() : base()
        {

        }

        public override ushort VoltronPacketType => (ushort)TSO_PlayTest_VoltronPacketTypes.DBRequestWrapperPDU;

        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.SendingAvatarID"/>
        /// </summary>
        public uint SenderAvatarID { get; set; }
        /// <summary>
        /// m_PlayerInfo
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set;  }
        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.Message"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }
    }
}

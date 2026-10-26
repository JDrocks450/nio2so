using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.DBRequestWrapperPDU)]
    public class TSODBRequestWrapperPDU : TSOVoltronPacket
    {
        public TSODBRequestWrapperPDU() : base()
        {

        }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.DBRequestWrapperPDU;

        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.SendingAvatarID"/>
        /// </summary>
        public uint SenderAvatarID { get; set; }
        /// <summary>
        /// m_PlayerInfo
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set;  }
        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ContentLength { get; set; }
        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.Message"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }
    }
}

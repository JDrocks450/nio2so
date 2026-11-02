using nio2so.Voltron.Core.TSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.LotEntryRequestPDU)]
    internal class TSOLotEntryRequestPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.LotEntryRequestPDU;
        public TSOLotEntryRequestPDU()
        {
            MakeBodyFromProperties();
        }

        public uint SendingAvatarID { get; set; }
        /// <summary>
        /// m_GridXY
        /// </summary>
        public uint GridXY { get; set; }

        /// 
        /// mpHouseSimsConstraints?
        ///
        
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Pascal)]        
        public string Attribute { get; set; }
    }
}

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
        public TSOFlashMessageResponsePDU() : base() { MakeBodyFromProperties(); }

        public TSOFlashMessageResponsePDU(TSOAriesIDStruct RecipientID, TSOPlayerInfoStruct PlayerInfo, string MessageText, uint StatusCode = 0x0, bool WasPersisted = true) : this()
        {
            this.WasPersisted = WasPersisted;
            this.RecipientID = RecipientID;
            this.PlayerInfo = PlayerInfo;
            this.MessageText = MessageText;
            this.StatusCode = StatusCode;
            this.WasPersisted = WasPersisted;
            MakeBodyFromProperties();
        }

        public TSOStatusReasonStruct StatusReason { get; set; } = TSOStatusReasonStruct.Online;
        public bool WasPersisted { get; set; } = false;
        public uint StatusCode { get; set; } = 0x0;
        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the intended recipient of the message
        /// </summary>
        public TSOAriesIDStruct RecipientID { get; set; }   
        /// <summary>
        /// The <see cref="TSOAriesIDStruct"/> of the sender of the message
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set; }
              
        public string MessageText { get; set; }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_RESPONSE_PDU;
    }
}

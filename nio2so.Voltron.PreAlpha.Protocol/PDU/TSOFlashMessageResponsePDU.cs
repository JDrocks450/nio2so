using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.FLASH_MSG_RESPONSE_PDU)]
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

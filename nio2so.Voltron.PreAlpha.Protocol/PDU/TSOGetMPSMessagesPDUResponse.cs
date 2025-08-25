using nio2so.Data.Common.Testing;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_RESPONSE_PDU)]
    public class TSOGetMPSMessagesResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_RESPONSE_PDU;
        
        TSOStatusReasonStruct StatusReason = new TSOStatusReasonStruct();

        public TSOGetMPSMessagesResponsePDU(TSOStatusReasonStruct Status) : base()
        {
            StatusReason = Status;
            MakeBodyFromProperties();
        }
    }
}

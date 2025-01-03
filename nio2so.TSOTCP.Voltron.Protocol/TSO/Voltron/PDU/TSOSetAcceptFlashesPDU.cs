using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU)]
    internal class TSOSetAcceptFlashesPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_PDU;
        public TSOSetAcceptFlashesPDU(uint value = 0x0)
        {
            Value = value;
            MakeBodyFromProperties();
        }

        public TSOSetAcceptFlashesPDU()
        {
        }

        public uint Value { get; set; }
    }
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_RESPONSE_PDU)]
    internal class TSOSetAcceptFlashesResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_FLASHES_RESPONSE_PDU;

        public TSOSetAcceptFlashesResponsePDU(bool AcceptsFlashes, uint statusCode = TSOVoltronConst.ResponsePDU_DefaultStatusCode,
            string reasonText = TSOVoltronConst.ResponsePDU_DefaultReasonText)
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            AcceptingFlashes = (byte)(AcceptsFlashes ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public byte AcceptingFlashes { get; set; } = 0x0;
    }
}

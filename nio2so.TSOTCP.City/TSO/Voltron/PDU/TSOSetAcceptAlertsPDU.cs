using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU)]
    internal class TSOSetAcceptAlertsPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptAlertsPDU()
        {
        }
        public TSOSetAcceptAlertsPDU(uint value=0x0)
        {
            Value = value;
            MakeBodyFromProperties();
        }

        public uint Value { get; set; }
    }
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_RESPONSE)]
    internal class TSOSetAcceptAlertsResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.SET_ACCEPT_ALERTS_PDU;
        public TSOSetAcceptAlertsResponsePDU(bool AcceptsAlerts, uint statusCode = 200, string reasonText = "OK.")
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
            AcceptingAlerts = (byte)(AcceptsAlerts ? 1 : 0);
            MakeBodyFromProperties();
        }

        public uint StatusCode { get; set; }
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)]
        public string ReasonText { get; set; }
        public byte AcceptingAlerts { get; set; } = 0x0;
    }
}

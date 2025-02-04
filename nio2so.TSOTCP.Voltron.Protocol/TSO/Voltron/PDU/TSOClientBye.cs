﻿using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// This PDU is sent when the client is saying bye to the server.
    /// <para>From CAS, when you're finished with your changes you have the option to return to SAS. </para>
    /// <para>Since SAS is connected to TSOHTTP and not Voltron, the Client connects to Voltron to show CAS, 
    /// then once finished sends it's payload to Voltron. This is sent afterwards so the Client notifies the Server 
    /// it is disconnecting on purpose.</para>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.BYE_PDU)]
    public class TSOClientBye : TSOVoltronPacket
    {
        public uint StatusCode { get; set; }
        [TSOVoltronString]
        public string Message { get; set; }

        public TSOClientBye() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOClientBye(uint statusCode, string message) : base()
        {
            StatusCode = statusCode;
            Message = message;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.BYE_PDU;
    }
}

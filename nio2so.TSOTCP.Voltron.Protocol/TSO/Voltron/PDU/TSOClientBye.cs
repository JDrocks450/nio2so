using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

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
        public TSOStatusReasonStruct StatusReason { get; set; }
        /// <summary>
        /// Creates a new <see cref="TSOClientBye"/>
        /// </summary>
        public TSOClientBye() : base()
        {
            MakeBodyFromProperties();
        }
        /// <summary>
        /// <inheritdoc cref="TSOClientBye()"/> with the <paramref name="statusCode"/> and <paramref name="message"/>
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public TSOClientBye(uint statusCode, string message) : this(new(statusCode, message)) { }
        /// <summary>
        /// <inheritdoc cref="TSOClientBye(uint,string)"/>
        /// </summary>
        /// <param name="StatusReason"></param>
        public TSOClientBye(TSOStatusReasonStruct StatusReason) : this()
        {
            this.StatusReason = StatusReason;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.BYE_PDU;
    }
}

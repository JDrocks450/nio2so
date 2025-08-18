using nio2so.Voltron.Core.TSO;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// This is a <see cref="TSODBRequestWrapper"/> that has not been implemented in nio2so yet, 
    /// it will dump the contents of the message past the header into the MessageContent property.
    /// </summary>
    [TSOVoltronPDU((uint)TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU)]
    public class TSODefaultDBWrapperPDU : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField][TSOVoltronBodyArray] public byte[] MessageContent { get; set; }

        public TSODefaultDBWrapperPDU() : base() { }
    }
}

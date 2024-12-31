using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// This is a <see cref="TSODBRequestWrapper"/> that has not been implemented in nio2so yet, 
    /// it will dump the contents of the message past the header into the MessageContent property.
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU)]
    internal class TSODefaultDBWrapperPDU : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField][TSOVoltronBodyArray] public byte[] MessageContent { get; set; }

        public TSODefaultDBWrapperPDU() : base() { }
    }
}

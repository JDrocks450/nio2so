using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{

    internal class TSOAvatarCreationRequest : TSOMessageFormatBase
    {
        public override TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType() => TSO_NewImproved_TSOMessageType_CLSIDs.cTSOAvatarCreationRequest;

        public override uint GetResponseID() => Parameter;

        public override void SetResponseID(uint newRespID) { }

        public uint Parameter { get; set; } = 0x01;
        [TSOVoltronBodyArray]
        public byte[] AvatarDataStream { get; set; } = Array.Empty<byte>();
    }
}

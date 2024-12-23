using nio2so.Formats.Util.Endian;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob.Structures
{
    /// <summary>
    /// This is a <see cref="TSOGenericDataBlobContent"/> that is a <see cref="TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage"/>
    /// as in, it has a byte followed by a kMSG then the payload data
    /// </summary>
    [TSOVoltronDatablobContent(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
    internal class TSOStandardMessageContent : ITSODataBlobContentObject
    {
        [TSOVoltronBroadcastDatablobPDUField] public byte BufferStartByte { get; set; } = 0x01;
        [TSOVoltronBroadcastDatablobPDUField] public TSO_PreAlpha_MasterConstantsTable kMSG { get; set; }
        [TSOVoltronBroadcastDatablobPDUField] [TSOVoltronBodyArray] public byte[] MessageContent { get; set; }

        public TSOStandardMessageContent() { }

        public TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable kMSG)
        {
            this.kMSG = kMSG;
            MessageContent = new byte[0];
        }
        public TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable kMSG, byte[] MessageContent) : this(kMSG) => this.MessageContent = MessageContent;

        public bool Match(TSO_PreAlpha_MasterConstantsTable kMSG) => this.kMSG == kMSG;
    }
}

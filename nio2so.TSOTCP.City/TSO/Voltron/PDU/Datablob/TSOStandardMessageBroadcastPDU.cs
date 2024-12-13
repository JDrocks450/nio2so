using nio2so.Formats.Util.Endian;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob
{
    /// <summary>
    /// This is a <see cref="TSOBroadcastDatablobPacket"/> that is a <see cref="TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage"/>
    /// as in, it has a byte followed by a kMSG then the payload data
    /// </summary>
    [TSOVoltronBroadcastDatablobPDU(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
    internal class TSOStandardMessageBroadcastPDU : TSOBroadcastDatablobPacket
    {
        [TSOVoltronBroadcastDatablobPDUField] public byte BufferStartByte { get; set; } = 0x01;
        [TSOVoltronBroadcastDatablobPDUField] public TSO_PreAlpha_MasterConstantsTable kMSG { get; set; }
        [TSOVoltronBroadcastDatablobPDUField] [TSOVoltronBodyArray] public byte[] MessageContent { get; set; }

        public TSOStandardMessageBroadcastPDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSOStandardMessageBroadcastPDU(TSO_PreAlpha_MasterConstantsTable kMSG, byte[] MessageContent) : base(
                TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage
            )
        {
            this.kMSG = kMSG;
            this.MessageContent = MessageContent;
        }

        public bool Match(TSO_PreAlpha_MasterConstantsTable kMSG) => this.kMSG == kMSG;                
    }
}

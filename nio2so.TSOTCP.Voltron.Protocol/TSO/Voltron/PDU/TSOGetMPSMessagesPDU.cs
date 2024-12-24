using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    /// <summary>
    /// This message is sent when the Client is requesting to get all messages in their Inbox
    /// <para/>It is unclear what 'MPS' may mean, but their exists too 'BBS' messages in the constants table.
    /// <para/>Best guess: MPS stands for 'Message Protocol System/Service'
    /// <para/>See also: <see cref="TSOGetMPSMessagesPDUResponse"/>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_PDU)]
    internal class TSOGetMPSMessagesPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.GET_MPS_MESSAGES_PDU;

        //**blank?
        //incoming packets show it just sends 0x0052 as a packet type, the packet size, and that's it.
        //putting this here for clarity only: (packet bytes)
        //00 52 00 00 00 06
        //size is six. type is 0x52. no room for any more data. weird. 
        //I guess this regulator in the original builds uses IP address?
        //idk

        public TSOGetMPSMessagesPDU() : base() { }
    }
}

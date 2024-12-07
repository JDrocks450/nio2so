using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob
{
    /// <summary>
    /// Basically packs and ships data of a defined format to the server with the intention to send it to all connected parties
    /// in the Room Server
    /// <para/>As a sidenote (not like anyone reads these but me any way) why is everything called a blob? idk me, why don't you ask them?
    /// well i tried but this was made 20 years ago. why am i even doing this?
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU)]
    internal class TSOBroadcastDatablobPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU;

        [TSOVoltronString] public string AriesID { get; set; } = "";
        [TSOVoltronString] public string MasterID { get; set; } = "";
        public ushort Arg1 { get; set; }   
        public uint MessageLength { get; set; }
        public TSO_PreAlpha_DBStructCLSIDs Struct {  get; set; }
        public byte Byte1 { get; set; }
        public TSO_PreAlpha_MasterConstantsTable kMSG { get; set; }
        [TSOVoltronBodyArray] public byte[] MessageContent { get; set; } = new byte[0];

        public TSOBroadcastDatablobPDU() : base()
        {

        }

        public override string ToShortString(string Arguments = "") => ToString();
        public override string ToString()
        {
            return $"{GetType().Name}({kMSG}, byte[{MessageContent.Length}])";
        }
    }
}

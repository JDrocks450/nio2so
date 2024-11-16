using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU)]
    internal class TSOFindPlayerPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.FIND_PLAYER_PDU;

        [TSOVoltronString()] public string AriesID { get; set; } = "";
        [TSOVoltronString()] public string MasterID { get; set; } = "";

        public TSOFindPlayerPDU() : base() { }

        public TSOFindPlayerPDU(string ariesID, string masterID) : base()
        {
            AriesID = ariesID;
            MasterID = masterID;
            MakeBodyFromProperties();
        }
    }
}

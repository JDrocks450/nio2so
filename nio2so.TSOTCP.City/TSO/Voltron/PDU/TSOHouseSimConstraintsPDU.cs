using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [TSOVoltronPacketAssociation(TSO_PreAlpha_VoltronPacketTypes.HOUSE_SIM_CONSTRAINTS_RESPONSE_PDU)]
    internal class TSOHouseSimConstraintsResponsePDU : TSOVoltronPacket
    {
        public TSOHouseSimConstraintsResponsePDU() : base() { }
        public TSOHouseSimConstraintsResponsePDU(uint arg1) : this()
        {
            Arg1 = arg1;
            MakeBodyFromProperties();
        }

        public uint Arg1 { get; set; } = 0x0000053A;
    }
}

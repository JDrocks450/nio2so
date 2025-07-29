using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.LIST_OCCUPANTS_RESPONSE_PDU)]
    public class TSOListOccupantsResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.LIST_OCCUPANTS_RESPONSE_PDU;

        public uint StatusCode { get; set; } = 0;
        public string ReasonText { get; set; } = "";
        public TSORoomIDStruct RoomID { get; set; } = new();
        [TSOVoltronArrayLength(nameof(Occupants))] public ushort NumberOfOccupants { get; set; }
        public TSOPlayerInfoStruct[] Occupants { get; set; } = Array.Empty<TSOPlayerInfoStruct>();

        public TSOListOccupantsResponsePDU() : base()
        {
        }

        public TSOListOccupantsResponsePDU(TSORoomIDStruct roomID, params TSOPlayerInfoStruct[] occupants) : this()
        {
            StatusCode = 0;
            ReasonText = "";
            RoomID = roomID;
            Occupants = occupants;
            MakeBodyFromProperties();
        }
    }
}

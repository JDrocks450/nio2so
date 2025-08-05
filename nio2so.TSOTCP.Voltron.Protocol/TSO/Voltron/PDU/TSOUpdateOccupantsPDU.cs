using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_OCCUPANTS_PDU)]
    public class TSOUpdateOccupantsPDU : TSOVoltronPacket
    {
        public uint RequestStatus { get; set; }
        public TSORoomInfoStruct RoomInfo { get; set; } = TSORoomInfoStruct.NoRoom;
        [TSOVoltronArrayLength(nameof(Occupants))] public ushort OccupantsCount { get; set; }
        public TSOPlayerInfoStruct[] Occupants { get; set; } = Array.Empty<TSOPlayerInfoStruct>();

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_OCCUPANTS_PDU;

        public TSOUpdateOccupantsPDU(TSORoomInfoStruct roomInfo, params TSOPlayerInfoStruct[] occupants) : this()
        {
            RequestStatus = 0;

            RoomInfo = roomInfo;
            Occupants = occupants;
            MakeBodyFromProperties();
        }

        public TSOUpdateOccupantsPDU() : base()
        {
        }
    }
}

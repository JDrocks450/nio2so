using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.HouseSimConstraintsPDU)]
    internal class TSOHouseSimConstraintsPDU : TSOVoltronPacket
    {
        public TSOHouseSimConstraintsPDU()
        {
        }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.HouseSimConstraintsPDU;

        /// <summary>
        /// Equal to 0x8000
        /// </summary>
        public ushort ParameterCountTopWord { get; set; } = 0x8000;
        [Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes.TSOVoltronArrayLength(nameof(Parameters))]
        public ushort ParameterCount { get; set; }
        public TSOStringPair[] Parameters { get; set; } = Array.Empty<TSOStringPair>();
    }

    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.HouseSimConstraintsResponsePDU)]
    internal class TSOHouseSimConstraintsResponsePDU : TSOVoltronPacket
    {
        public TSOHouseSimConstraintsResponsePDU() : base()
        {
        }
        public TSOHouseSimConstraintsResponsePDU(uint SendingAvatarID) : this()
        {
            this.SendingAvatarID = SendingAvatarID;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.HouseSimConstraintsResponsePDU;

        /// <summary>
        /// Equal to 0x8000
        /// </summary>
        public uint SendingAvatarID { get; set; } = 0x8000;
    }
}

using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.RSGZWrapperPDU)]
    internal class TSORSGZWrapperPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.RSGZWrapperPDU;

        public uint SendingAvatarID { get; set; }
        public uint MessageID { get; set; }
        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ContentLength { get; set; }
        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.Message"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }
    }
}

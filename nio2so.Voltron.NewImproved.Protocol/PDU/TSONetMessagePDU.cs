using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.cTSONetMessagePDU)]
    internal class TSONetMessagePDU : TSOVoltronPacket
    {
        public TSONetMessagePDU() : base()
        {
        }

        public TSONetMessagePDU(uint sendingAvatarID, TSOServiceMessage message, params uint[] vector) : this()
        {
            SendingAvatarID = sendingAvatarID;
            StatusCode = 0x0;
            Vector = vector;
            Message = message;

            MakeBodyFromProperties();
        }
        /// <summary>
        /// Creates a new <see cref="TSONetMessagePDU"/> with the given <paramref name="kMSGID"/> and <paramref name="message"/>
        /// <para/>In niotso, this is populated with 3 <c>0x3b</c> values surrounding <paramref name="kMSGID"/>. This has been brought forward for now.
        /// <para/>It is unsure why it is doing this or if it is even needed.
        /// </summary>
        /// <param name="sendingAvatarID"></param>
        /// <param name="kMSGID"></param>
        /// <param name="message"></param>
        public TSONetMessagePDU(uint sendingAvatarID, uint kMSGID, TSOServiceMessage message) : 
            this(sendingAvatarID, message, 0x3b, 0x3b, kMSGID, 0x3b)
        { }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.cTSONetMessagePDU;

        public uint SendingAvatarID { get; set; }
        /// <summary>
        /// The amount of items in the following <see cref="Vector"/> -- automatically populated by <see cref="TSOVoltronSerializer"/>
        /// </summary>
        [TSOVoltronArrayLength(nameof(Vector))]
        public uint VectorCount { get; set; }
        public uint StatusCode { get; set; }        
        /// <summary>
        /// Fields on the <see cref="TSONetMessagePDU"/> giving context around the ObjectID, FieldID, TypeID, etc.
        /// </summary>
        public uint[] Vector { get; set; } = Array.Empty<uint>();
        /// <summary>
        /// The enclosed <see cref="TSOServiceMessage"/> in this <see cref="TSONetMessagePDU"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }
    }
}

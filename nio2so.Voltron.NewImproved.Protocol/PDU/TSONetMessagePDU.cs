using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        /// <summary>
        /// Creates a new <see cref="TSONetMessagePDU"/> with the given <paramref name="kMSGID"/> and <paramref name="message"/>
        /// </summary>
        public TSONetMessagePDU(uint sendingAvatarID, TSO_NewImproved_kMSGs kMSGID, TSOServiceMessage message, uint Parameter = 0x02) : this()
        {
            this.kMSGID = kMSGID;
            Message = message;
            MakeBodyFromProperties();

            uint adjustedLength = ContentLength + 8;

            this.Parameter = Parameter;
            Size1 = adjustedLength;
            Size2 = 0x80000000 | adjustedLength;            

            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.cTSONetMessagePDU;

        public uint SendingAvatarID { get; set; }
        /// <summary>
        /// The amount of items in the following <see cref="Vector"/> -- automatically populated by <see cref="TSOVoltronSerializer"/>
        /// </summary>
        public uint Parameter { get; set; } = 0x02;
        public uint StatusCode { get; set; }        
        /// <summary>
        /// The size of all data including <see cref="kMSGID"/> onward.
        /// </summary>
        public uint Size1 { get; set; }
        /// <summary>
        /// The size of all data including <see cref="kMSGID"/> onward.
        /// <para/> 0x8000[<see cref="Size1"/>] - example: <c>0x80000050</c>
        /// </summary>
        public uint Size2 { get; set; }

        public TSO_NewImproved_kMSGs kMSGID { get; set; }

        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ContentLength { get; set; }

        /// <summary>
        /// The enclosed <see cref="TSOServiceMessage"/> in this <see cref="TSONetMessagePDU"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }
    }
}

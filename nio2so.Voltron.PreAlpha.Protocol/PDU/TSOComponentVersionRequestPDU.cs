using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU(ID)]
    internal class TSOComponentVersionRequestPDU : TSOVoltronPacket
    {
        private const ushort ID = (ushort)TSO_PreAlpha_VoltronPacketTypes.COMPONENT_VERSION_REQUEST_PDU;

        public const string MyComponentName = "ServerRuntime";
        public const string PreAlphaRuntimeVersion = "1.3.1.56";

        public TSOComponentVersionRequestPDU() {
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => ID;
        /// <summary>
        /// <inheritdoc cref="VersionStringsLength"/>
        /// <para/>This is the most significant bit, flipped.
        /// </summary>
        public byte VersionStringLengthUpperByte { get; set; } = 0x80;
        /// <summary>
        /// A 2-byte unsigned integer with the most significant bit flipped specifying the number of string pairs to follow
        /// <para/>This is the lower byte, with the number of string pairs
        /// </summary>
        [TSOVoltronArrayLength(nameof(VersionStrings))] public byte VersionStringsLength { get; set; }
        /// <summary>
        /// Each pair of strings in this <see cref="TSOComponentVersionRequestPDU"/>
        /// </summary>
        public TSOStringPair[] VersionStrings { get; set; } = [new TSOStringPair(MyComponentName, PreAlphaRuntimeVersion)];

        public override string ToString() => $"TSOClient Component Version: {string.Join(", ", (IEnumerable<TSOStringPair>)VersionStrings)}";
    }
}

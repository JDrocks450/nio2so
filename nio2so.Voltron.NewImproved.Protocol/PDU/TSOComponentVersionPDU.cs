using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.ComponentVersionRequestPDU)]
    internal class TSOComponentVersionRequestPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.ComponentVersionRequestPDU;
        public TSOComponentVersionRequestPDU()
        {
            MakeBodyFromProperties();
        }

        public uint SendingAvatarID { get; set; }
    }
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.ComponentVersionResponsePDU)]
    internal class TSOComponentVersionResponsePDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.ComponentVersionResponsePDU;
        public TSOComponentVersionResponsePDU()
        {
            MakeBodyFromProperties();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendingAvatarID"></param>
        /// <param name="versionStrings"><inheritdoc cref="VersionStrings"/></param>
        public TSOComponentVersionResponsePDU(uint sendingAvatarID, params TSOStringPair[] versionStrings)
        {
            SendingAvatarID = sendingAvatarID;
            if (VersionStrings.Any())
                VersionStrings = versionStrings;

            MakeBodyFromProperties();
        }

        public const string MyComponentName = "ServerRuntime";
        public const string NewImprovedRuntimeVersion = "1.3.1.56";

        public uint SendingAvatarID { get; set; }
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
        public TSOStringPair[] VersionStrings { get; set; } = [new TSOStringPair(MyComponentName, NewImprovedRuntimeVersion)];

        public override string ToString() => $"TSOClient Component Version: {string.Join(", ", (IEnumerable<TSOStringPair>)VersionStrings)}";
    }
}

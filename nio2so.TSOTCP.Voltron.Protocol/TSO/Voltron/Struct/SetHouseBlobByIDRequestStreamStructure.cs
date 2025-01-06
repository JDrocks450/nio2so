using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    /// <summary>
    /// Represents the structure of the TSOSerializableStream included in the <see cref="TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request"/>
    /// upon deserialization.
    /// </summary>
    [Serializable]
    public class SetHouseBlobByIDRequestStreamStructure
    {
        //*TOC_*

        /// <summary>
        /// Is a header, sent in little-endian format reading _COT.
        /// <para/> When in big-endian, would read TOC_ which is the way the game will properly read it.
        /// </summary>
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint TOC_StreamHeader { get; set; } = 0x5F434F54; //"_COT"
        /// <summary>
        /// This may be a version code of the House Blob data.
        /// <para/> Currently is unknown what it corresponds to.
        /// </summary>
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public ushort Arg1 { get; set; } = 0x02;

        //*CHUNKS*

        /// <summary>
        /// The HouseFile sent over the airwaves -- as in, chunks of a House File that contains the HouseBlob itself
        /// </summary>
        public TSOHouseChunkPack ChunkPackage { get; set; } = new();
    }
}

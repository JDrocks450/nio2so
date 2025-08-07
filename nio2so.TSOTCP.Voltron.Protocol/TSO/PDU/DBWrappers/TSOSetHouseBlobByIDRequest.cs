using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Streams;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    /// <summary>
    /// This PDU is used when the remote party wants to overwrite the <see cref="TSODBHouseBlob"/> stored in 
    /// the Database
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request)]
    public class TSOSetHouseBlobByIDRequest : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        /// <summary>
        /// 0x00000001 ?? no idea
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Parameter1 { get; set; } = 0x1;
        /// <summary>
        /// Length to end of payload
        /// </summary>
        [TSOVoltronDBWrapperField] public uint SARLength { get; set; }
        [TSOVoltronDBWrapperField] public uint SARHeader { get; set; } = 0x5F534152; // "_SAR"
        /// <summary>
        /// This is the <see cref="DecompressedSize"/> + the length of all bytes from HouseID (inclusive) to this field (inclusive) (20 bytes)
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint TotalSize { get; set; }
        /// <summary>
        /// Seems to be 0x02 as a uint -- unsure
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint CompressionType2 { get; set; }

        //TSO Serializable Stream
        /// <summary>
        /// A <see cref="TSOSerializableStream"/> containing a <see cref="SetHouseBlobByIDRequestStreamStructure"/> object
        /// <para/>See: <see cref="TryUnpack(out SetHouseBlobByIDRequestStreamStructure?)"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public TSOSerializableStream? HouseFileStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseFileStream;

        public TSOSetHouseBlobByIDRequest() : base() { }

        /// <summary>
        /// Decompresses the <see cref="HouseFileStream"/> to a <see cref="SetHouseBlobByIDRequestStreamStructure"/> instance
        /// </summary>
        /// <param name="Structure"></param>
        /// <returns></returns>
        public bool TryUnpack(out SetHouseBlobByIDRequestStreamStructure? Structure) => ((ITSOSerializableStreamPDU)this).TryUnpackStream(out Structure);
    }
}

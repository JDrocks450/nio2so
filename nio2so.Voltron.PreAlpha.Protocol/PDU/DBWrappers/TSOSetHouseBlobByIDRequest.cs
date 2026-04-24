using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Streams;
using nio2so.Voltron.Core.TSO.Serialization.Types;
using nio2so.Voltron.PreAlpha.Protocol.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// This PDU is used when the remote party wants to overwrite the <see cref="TSODBHouseBlob"/> stored in 
    /// the Database
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request)]
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

#if false
        //TSO Serializable Stream
        /// <summary>
        /// A <see cref="TSOSerializableStream"/> containing a <see cref="SetHouseBlobByIDRequestStreamStructure"/> object
        /// <para/>See: <see cref="TryUnpack(out SetHouseBlobByIDRequestStreamStructure?)"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public TSOSerializableStream? HouseFileStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseFileStream;
#else
        [TSOVoltronDBWrapperField] public CompressedRASStream HouseFileStream { get; set; } = new();
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseFileStream.CompressedStream;
#endif

        public TSOSetHouseBlobByIDRequest() : base() { }

        public bool TryUnpack(out RASStream.RASArchive? RASFile) => ((ITSOSerializableStreamPDU)this).TryUnpackStream(out RASFile);

        /// <summary>
        /// Decompresses the <see cref="HouseFileStream"/> to a <see cref="SetHouseBlobByIDRequestStreamStructure"/> instance
        /// </summary>
        /// <param name="Structure"></param>
        /// <returns></returns>
        public bool TryUnpack(out SetHouseBlobByIDRequestStreamStructure? Structure) => ((ITSOSerializableStreamPDU)this).TryUnpackStream(out Structure);
    }
}

using nio2so.Formats.Streams;
using nio2so.Voltron.Core.TSO.Serialization.Types;

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

        //TSO Serializable Stream
#if false
        
#else
        [TSOVoltronDBWrapperField] public CompressedRASStream HouseFileStream { get; set; } = new();
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseFileStream.CompressedStream;
#endif

        public TSOSetHouseBlobByIDRequest() : base() { }

        /// <summary>
        /// Decompresses the <see cref="HouseFileStream"/> to a <see cref="RASStream.RASArchive"/>
        /// </summary>
        /// <param name="Structure"></param>
        public bool TryUnpack(out RASStream.RASArchive? RASFile) => ((ITSOSerializableStreamPDU)this).TryUnpackStream(out RASFile);
    }
}

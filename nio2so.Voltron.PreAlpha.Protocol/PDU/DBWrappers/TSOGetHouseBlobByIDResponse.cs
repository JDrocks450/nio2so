#define RASTEST
#undef RASTEST

using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.DataService.Common.Types.Lot;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.Core.TSO.Serialization.Types;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Streams;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// The response packet to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request"/> which will provide
    /// the remote party with the stored <see cref="TSODBHouseBlob"/> data in the Database
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    public class TSOGetHouseBlobByIDResponse : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public bool HouseBlobFollows { get; set; } = true;

        //**

        [TSOVoltronDBWrapperField]
        public uint RASLength
        {
            get => (uint)(5 + (HouseBlobStream?.Length ?? 0) + FOOTERLEN); set => _ = value;
        }

        //**TSOSERIALIZABLE
        /// <summary>
        /// This is documented in HOUS_SMASH but it is the Hous chunk from a saved property file that can be found in UserData/Houses
        /// </summary>
#if RASTEST
        [TSOVoltronDBWrapperField] public CompressedRASStream HouseBlobStream { get; set; } = new();
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseBlobStream.CompressedStream;
#else
        [TSOVoltronDBWrapperField] public RASStream.RASHeader RASHeader { get; set; } = new() { Version = 0x0, Unknown = 0x2CE9 }; // "RAS\0"
        [TSOVoltronDBWrapperField] public TSOSerializableStream HouseBlobStream { get; set; } = new();
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseBlobStream;
#endif

        //**FOOTER

        const uint FOOTERLEN = sizeof(uint) * 1;


        [TSOVoltronDBWrapperField] public uint Footer1 { get; set; } = 0x01; //0x0036FCFC; // 0x01
        [TSOVoltronDBWrapperField] public uint Footer2 { get; set; } = 0xDADDE510;
        [TSOVoltronDBWrapperField] public uint Footer3 { get; set; } = 0xDADDE511;

#if false
        [TSOVoltronDBWrapperField]
        public byte[] FooterGarbage
        {
            get => new byte[]
            {
                0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,
                0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,
                0xBA,0xAD,0xF0,0x0D
            };
            set => _ = value;
        }
#endif

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetHouseBlobByIDResponse() : base() { }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="houseID">The ID of the house</param> 
        /// <param name="HouseBlob">The house data blob</param> 
        /// <param name="CompressBlob">Flag indicating if the blob should be compressed</param>
        public TSOGetHouseBlobByIDResponse(uint houseID, TSODBHouseBlob HouseBlob, bool CompressBlob = true) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            HouseID = houseID;

            // Error handling for null HouseBlob
            if (HouseBlob == null) 
                throw new ArgumentNullException(nameof(HouseBlob));

            //Create the RAS Archive with the Hous chunk containing the HouseBlob data
            var HousChunkData = TSOVoltronSerializer.Serialize(HouseBlob);
            RASStream.RASArchive archive = new RASStream.RASArchive(
                new RASStream.RASArchive.RASChunk((uint)TSO_PreAlpha_HouseStreamChunkHeaders.flag, [0,0,0,0]),
                new RASStream.RASArchive.RASChunk((uint)TSO_PreAlpha_HouseStreamChunkHeaders.hous, HousChunkData));

#if RASTEST
            HouseBlobStream = new CompressedRASStream(archive, CompressBlob, 0x9);
            HouseBlobStream.Header.Version = 0x0;
            HouseBlobStream.Header.Unknown = 0x2CE9;
#else
            byte[] Data = archive.GetChunk((uint)TSO_PreAlpha_HouseStreamChunkHeaders.hous).Content;
            if (CompressBlob)
                HouseBlobStream = TSOSerializableStream.ToCompressedStream(Data);
            else
                HouseBlobStream = new TSOSerializableStream(0x01, Data, (uint)Data.Length);
            RASHeader.TotalSize = (uint)(HouseBlobStream.Length + 5); // the length of the stream + the 5 bytes in the header
            RASHeader.Unknown = (ushort)HouseBlobStream?.GetTotalLength();
#endif

            MakeBodyFromProperties();
        }
    }
}

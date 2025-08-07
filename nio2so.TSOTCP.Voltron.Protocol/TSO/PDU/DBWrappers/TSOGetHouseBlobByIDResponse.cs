#define ATTEMPT_2

using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Streams;
using nio2so.DataService.Common.Types.Lot;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Serialization;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    /// <summary>
    /// The response packet to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Request"/> which will provide
    /// the remote party with the stored <see cref="TSODBHouseBlob"/> data in the Database
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    public class TSOGetHouseBlobByIDResponse : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public byte Filler1 { get; set; } = 0x01;

        //**

        [TSOVoltronDBWrapperField]
        public uint Filler4
        {
            get => (uint)(0x1D + (HouseBlobStream?.Length ?? 0) + FOOTERLEN); set => _ = value;
        }
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.BigEndian)] public uint Filler5 { get; set; } = 0x5F534152;
        
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        public uint Filler6
        {
            get => HouseBlobStream?.DecompressedSize ?? 0 + 0x11;
            set => _ = value;
        }

        [TSOVoltronDBWrapperField]
        public uint HB_Payload_Size
        {
            get => HouseBlobStream?.GetTotalLength() ?? 0;
            set => _ = value;
        }

        //**TSOSERIALIZABLE
        /// <summary>
        /// This is documented in HOUS_SMASH but it is the Hous chunk from a saved property file that can be found in UserData/Houses
        /// </summary>
        [TSOVoltronDBWrapperField] public TSOSerializableStream HouseBlobStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => HouseBlobStream;

        //**FOOTER

        const uint FOOTERLEN = sizeof(uint) * 1;

        [TSOVoltronDBWrapperField] public uint Footer1 { get; set; } = 0x01; //0x0036FCFC; // 0x01
        [TSOVoltronDBWrapperField] public uint Footer2 { get; set; } = 0xDADDE510;
        [TSOVoltronDBWrapperField] public uint Footer3 { get; set; } = 0xDADDE511;

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

            var decompressedBytes = TSOVoltronSerializer.Serialize(HouseBlob);
            if (CompressBlob)
                HouseBlobStream = TSOSerializableStream.ToCompressedStream(decompressedBytes);
            else
                HouseBlobStream = new TSOSerializableStream(0x01, decompressedBytes, 0x200C);

            MakeBodyFromProperties();
        }
    }
}

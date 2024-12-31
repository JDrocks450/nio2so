#define ATTEMPT_2

using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;
using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.DB;
using nio2so.Formats.Streams;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using nio2so.TSOTCP.City.TSO.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response)]
    internal class TSOGetHouseBlobByIDResponse : TSODBRequestWrapper, ITSOSerializableStreamPDU
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public byte Filler1 { get; set; } = 0x01;

        //**

        [TSOVoltronDBWrapperField] public uint Filler4 => (uint)(0x1D + (HouseBlobStream?.Length ?? 0) + FOOTERLEN);
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.BigEndian)] public uint Filler5 => 0x5F534152;
        [TSOVoltronDBWrapperField][TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint Filler6 => HouseBlobStream?.DecompressedSize ?? 0 + 0x11;
        [TSOVoltronDBWrapperField] public uint HB_Payload_Size => HouseBlobStream?.GetTotalLength() ?? 0;

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
        public byte[] FooterGarbage => new byte[]
        {
            0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,
            0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,0xBA,0xAD,0xF0,0x0D,
            0xBA,0xAD,0xF0,0x0D
        };

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetHouseBlobByIDResponse(uint houseID, TSODBHouseBlob HouseBlob, bool CompressBlob = true) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            HouseID = houseID;

            var decompressedBytes = TSOVoltronSerializer.Serialize(HouseBlob);
            if (CompressBlob)
                HouseBlobStream = TSOSerializableStream.ToCompressedStream(decompressedBytes);
            else
                HouseBlobStream = new TSOSerializableStream(0x01, decompressedBytes, 0x200C);

            MakeBodyFromProperties();
        }
    }
}

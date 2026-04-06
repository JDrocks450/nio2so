using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    //6C 3B 73 64 66 6B 6C 3B <-- LOOK FOR THIS
    //

    /* 
     * RESPONSE PACKET NOTES
     * UINT NumOfItems? Seems to activate Sim button if clicking the house search if it is greater than one? could be proceeding data tho
     * UINT UNKN
     * UINT UNKN
     * LENGTH_BYTE_STRING NAME?
     * UINT DATA_SIZE will read these bytes and the next item in the list after this will be read exactly this many bytes from this uint (exclusive)
     */

    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response)]
    public class TSOGetTopListResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// <see cref="UInt32"/> values corresponding to <see cref="TSOTop100List.ListType"/> property
        /// </summary>
        public enum TSOTop100ListTypes : uint
        {
            Avatars = 0x0001,
            Houses = 0x0002,
            Clubs = 0x0003,
            Neighborhoods = 0x0004
        }

        public record TSOTop100List
        {
            /// <summary>
            /// Creates a new <see cref="TSOTop100List"/>
            /// <para/><paramref name="ListID"/> <inheritdoc cref="ListID"/>
            /// </summary>
            /// <param name="ListID"></param>
            /// <param name="ListType"></param>
            /// <param name="ListName"></param>
            /// <param name="BMPBytes"></param>
            public TSOTop100List(uint ListID, TSOTop100ListTypes ListType, string ListName, byte[] BMPBytes)
            {
                this.ListID = ListID;
                this.ListType = (uint)ListType;
                ThumbnailBytes = BMPBytes;
                this.ListName = ListName;
            }
            /// <summary>
            /// The ID for this list in the database to use when referencing this <see cref="TSOTop100List"/>
            /// </summary>
            public uint ListID { get; set; }
            /// <summary>
            /// ListType maps to <see cref="TSOTop100ListTypes"/> enum values
            /// </summary>
            public uint ListType { get; set; }
            /// <summary>
            /// The display name for this <see cref="TSOTop100List"/>
            /// </summary>
            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string ListName { get; set; } = "";
            /// <summary>
            /// The <see cref="Array.Length"/> property of the <see cref="ThumbnailBytes"/> array.
            /// </summary>
            [TSOVoltronArrayLength(nameof(ThumbnailBytes))] public uint ThumbnailLength { get; set; }
            /// <summary>
            /// An array of bytes containing the BMP Thumbnail image for this <see cref="TSOTop100List"/>
            /// <para/>The BMP format must be an RLE8 encoded BMP image, which an encoder implementation is provided in the nio2so codebase.
            /// </summary>
            public byte[] ThumbnailBytes { get; set; } = [];
        }
        /// <summary>
        /// The number of items in the <see cref="Entries"/> list
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronArrayLength(nameof(Entries))] public uint EntriesCount { get; set; }
        /// <summary>
        /// The <see cref="TSOTop100List"/> entries in this <see cref="TSOGetTopListResponse"/>
        /// </summary>
        [TSOVoltronDBWrapperField] public TSOTop100List[] Entries { get; set; } = [];

        public TSOGetTopListResponse() :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response
                )
        {
            MakeBodyFromProperties();
        }
        public TSOGetTopListResponse(params TSOTop100List[] Lists) : this()
        {
            Entries = Lists;
        }
    }    
}

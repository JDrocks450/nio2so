using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.DataService.Common.Types.Avatar
{
    /// <summary>
    /// The decompressed payload of a <see cref="TSO_PreAlpha_DBActionCLSIDs.InsertNewCharBlob_Request"/> or a 
    /// <see cref="TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response"/> follows this structure
    /// </summary>
    public class TSODBCharBlob
    {
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint HeaderByte { get; set; } = 0x01;
        /// <summary>
        /// A string that always reads, "not needed".
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string NotNeeded { get; set; } = "not needed";
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)] public uint AvatarID { get; set; }
        /// <summary>
        /// Personality, Skills, Appearance, and smashed "[name]$[description] are included in this stream
        /// </summary>
        [TSOVoltronBodyArray] public byte[] CharBlobStream { get; set; }

        public TSODBCharBlob() : base() { }
        public TSODBCharBlob(uint AvatarID, byte[] DecompressedCharBlobData) : this()
        {
            CharBlobStream = DecompressedCharBlobData;
        }
    }
}

using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.DataService.Common.Types.Avatar
{
    /// <summary>
    /// Documented at 0x013F in the CharBlob file
    /// </summary>
    public class TSODBCharBlobDataObject1
    {
        byte unknown1 = 0x03;
        byte unknown2 = 0x00;
        uint uint1 = 0; // variable ...  could be time created?
        uint charfile1 = 0x0; // changing this crashes the game
        uint MyLotID = 0x0;
        uint charfile3 = 0x01;
        uint spacer1 = 0x04;
        uint charfile4 = 0x6F38D0FC;
        uint spacer2 = 0x04;
        uint funds = 0x0;
        uint unk1 = 0xBAADF00D;
        uint unk2 = 0xBAADF00D;
        uint unk3 = 0xBAADF00D;
    }
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

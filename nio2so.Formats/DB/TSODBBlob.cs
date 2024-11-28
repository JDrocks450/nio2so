using nio2so.Formats.Util.Endian;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// A data type not fully understood yet.
    /// </summary>
    public abstract class TSODBBlob
    {
        /// <summary>
        /// Creates a new <see cref="TSODBBlob"/> from the given data
        /// </summary>
        /// <param name="blobData"></param>
        protected TSODBBlob(byte[] blobData)
        {
            BlobData = blobData;
        }

        /// <summary>
        /// Since a <see cref="TSODBCharBlob"/> is a RefPack bitstream, this is the DecompressedSize of the RefPack located
        /// after the MagicNumber.
        /// </summary>
        public uint DecompressedSize
        {
            get
            {
                byte[] uintChars = BlobData.Skip(2).Take(3).ToArray();
                byte[] u32chars = new byte[sizeof(uint)]
                {
                    0x00,
                    uintChars[0],
                    uintChars[1],
                    uintChars[2]
                };
                return EndianBitConverter.Big.ToUInt32(u32chars, 0);
            }
        }

        public uint Length => (uint)BlobData.Length;

        /// <summary>
        /// The raw data in this <see cref="TSODBBlob"/>
        /// </summary>
        public byte[] BlobData { get; }
    }
}

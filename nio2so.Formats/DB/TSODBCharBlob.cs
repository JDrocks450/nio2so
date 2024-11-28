using nio2so.Formats.Util.Endian;
using System.Text;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// The body of a DBRequestWrapper PDU in Voltron sent in request/response to a (Get/Set)CharBlobByID command
    /// <para/>This is a RefPack bitstream and should start with 0x10FB
    /// </summary>
    public class TSODBCharBlob : TSODBBlob
    {
        public TSODBCharBlob(byte[] blobData) : base(blobData)
        {

        }

        /// <summary>
        /// This will run a simple test procedure to ensure the data in this blob is safe and formatted correctly.
        /// </summary>
        public void EnsureNoErrors()
        {
            if (BlobData.Length < 6) throw new InvalidDataException("BlobData is too small (less than 6 bytes)");
            if (BlobData[0] != 0x10 || BlobData[1] != 0xFB)
                throw new InvalidDataException("The provided BlobData stream doesn't start with 0x10FB which will cause issues.");
        }
    }
}

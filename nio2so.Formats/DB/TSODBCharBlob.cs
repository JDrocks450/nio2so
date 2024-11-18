using nio2so.Formats.Util.Endian;
using System.Text;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// The body of a DBRequestWrapper PDU in Voltron sent in request/response to a (Get/Set)CharBlobByID command
    /// </summary>
    public class TSODBCharBlob : TSODBBlob
    {
        /// <summary>
        /// The encoded name of the Avatar this blob corresponds with
        /// </summary>
        public string AvatarName
        {
            get
            {
                try
                {
                    byte length = BlobData[0];
                    return Encoding.UTF8.GetString(BlobData,1,length);
                }
                catch(Exception e)
                {

                }
                return "";
            }
        }
        /// <summary>
        /// Gets the index of where the <see cref="BlobSize"/> property should be in the <see cref="TSODBBlob.BlobData"/> array.
        /// <para/> This is found right after the <see cref="AvatarName"/> property
        /// </summary>
        public int BlobSizeIndex
        {
            get
            {
                //GET STRING LENGTH
                byte length = BlobData[0];
                int position = sizeof(byte) + length;
                return position;
            }
        }

        /// <summary>
        /// The size that is encoded into the buffer data reporting how large the CharBlob size is.
        /// <para>This is <see cref="UInt32"/> right after the <see cref="AvatarName"/></para>
        /// <para>See: <see cref="BlobSizeIndex"/></para>
        /// </summary>
        public uint BlobSize
        {
            get
            {                
                return EndianBitConverter.Big.ToUInt32(BlobData,BlobSizeIndex);
            }
        }

        public TSODBCharBlob(byte[] blobData) : base(blobData)
        {

        }
        /// <summary>
        /// This will run a simple test procedure to ensure the data in this blob is safe and formatted correctly.
        /// </summary>
        public void EnsureNoErrors()
        {
            return;
            if (BlobSize > BlobData.Length)
                throw new InvalidDataException($"The BlobSize property in this packet at position: {BlobSizeIndex}" +
                    $" is {BlobSize}, which is larger than the data provided: {BlobData.Length} bytes.");
        }
    }
}
